using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;

namespace eZet.EveProfiteer.Services {
    public class ShellService {
        private readonly EveApiService _eveApiService;

        private readonly TraceSource _trace = new TraceSource("EveProfiteer", SourceLevels.All);


        public ShellService(EveApiService eveApiService) {
            _eveApiService = eveApiService;
        }

        public async Task UpdateRefIdsAsync() {
            var reftypes = await _eveApiService.GetRefTypesAsync().ConfigureAwait(false);
            using (var db = getDb()) {
                db.RefTypes.RemoveRange(db.RefTypes);
                foreach (var apireftype in reftypes.Result.RefTypes) {
                    var reftype = db.RefTypes.Create();
                    ApiEntityMapper.Map(apireftype, reftype);
                    db.RefTypes.Add(reftype);
                }
                await db.SaveChangesAsync().ConfigureAwait(false);
            }
        }


        private EveProfiteerDbEntities getDb() {
            return IoC.Get<EveProfiteerDbEntities>();
        }

        private async Task<long> getLatestTransactionId() {
            using (EveProfiteerDbEntities db = getDb()) {
                return
                    await
                        db.Transactions.AsNoTracking()
                            .Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id)
                            .OrderByDescending(t => t.TransactionId)
                            .Select(t => t.TransactionId)
                            .FirstOrDefaultAsync().ConfigureAwait(false);
            }
        }

        private async Task<long> getLatestJournalId() {
            using (EveProfiteerDbEntities db = getDb()) {
                return
                    await
                        db.JournalEntries.AsNoTracking()
                            .Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id)
                            .OrderByDescending(t => t.RefId)
                            .Select(t => t.RefId)
                            .FirstOrDefaultAsync().ConfigureAwait(false);
            }
        }

        public async Task<int> UpdateAssetsAsync() {
            var result =
                await
                    _eveApiService.GetAssetsAsync(ApplicationHelper.ActiveKey, ApplicationHelper.ActiveKeyEntity)
                        .ConfigureAwait(false);
            var groups = result.Flatten().Where(asset => !asset.Singleton).GroupBy(asset => asset.TypeId).ToList();
            using (var db = getDb()) {
                var assets =
                    await
                        db.Assets.Where(asset => asset.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id)
                            .ToListAsync()
                            .ConfigureAwait(false);
                var lookup = assets.ToLookup(asset => asset.InvTypes_TypeId);
                foreach (var group in groups) {
                    var asset = lookup[group.Key].SingleOrDefault();
                    if (asset == null) {
                        asset = new Asset();
                        asset.InvTypes_TypeId = group.Key;
                        asset.ApiKeyEntity_Id = ApplicationHelper.ActiveKeyEntity.Id;
                        db.Assets.Add(asset);
                    }
                    asset.ActualQuantity = group.Sum(item => item.Quantity);
                    assets.Remove(asset);
                }
                foreach (var asset in assets) {
                    asset.ActualQuantity = 0;
                }
                _trace.TraceEvent(TraceEventType.Verbose, 0, "Processed {0} assets.", groups.Count());
                return await db.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task UpdateTransactionsAsync() {
            _trace.TraceEvent(TraceEventType.Start, 0, "StartUpdateTransactions");
            long latest = await getLatestTransactionId().ConfigureAwait(false);
            _trace.TraceEvent(TraceEventType.Verbose, 0, "Latest transaction ID: " + latest);
            var transactions =
                await
                    Task.Run(
                        () =>
                            _eveApiService.GetNewTransactionsAsync(ApplicationHelper.ActiveKey,
                                ApplicationHelper.ActiveKeyEntity, latest));
            _trace.TraceEvent(TraceEventType.Verbose, 0, "Fetched transactions: " + transactions.Count);
            await UpdateInventoryAsync(transactions).ConfigureAwait(false);
            await insertTransactions(transactions).ConfigureAwait(false);
            _trace.TraceEvent(TraceEventType.Stop, 0, "CompleteUpdateTransactions");
        }

        public async Task UpdateJournalAsync() {
            _trace.TraceEvent(TraceEventType.Start, 0, "StartUpdateJournal");
            long latest = await getLatestJournalId();
            _trace.TraceEvent(TraceEventType.Verbose, 0, "Latest journal ID: " + latest);
            var list =
                await
                    _eveApiService.GetNewJournalEntriesAsync(ApplicationHelper.ActiveKey,
                        ApplicationHelper.ActiveKeyEntity, latest);
            _trace.TraceEvent(TraceEventType.Verbose, 0, "Fetched journal entries: " + list.Count);
            await insertJournalEntries(list).ConfigureAwait(false);
            _trace.TraceEvent(TraceEventType.Stop, 0, "CompleteUpdateJournal");
        }

        public async Task CalculateInventoryAsync() {
            IList<Transaction> transactions;
            using (var db = getDb()) {
                var assets = await db.Assets.Where(asset => asset.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id).ToListAsync().ConfigureAwait(false);
                db.Assets.RemoveRange(assets);
                await db.SaveChangesAsync().ConfigureAwait(false);
                transactions = await db.Transactions.Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id).ToListAsync().ConfigureAwait(false);
            }
            await UpdateInventoryAsync(transactions);
        }

        private async Task insertJournalEntries(IEnumerable<JournalEntry> entries) {
            EveProfiteerDbEntities db;
            using (db = getDb()) {
                db.Configuration.AutoDetectChangesEnabled = false;
                IList<JournalEntry> list = entries.OrderBy(entry => entry.RefId).ToList();
                int count = 0;
                foreach (JournalEntry entry in list) {
                    db.JournalEntries.Add(entry);
                    ++count;
                    if (count % 5000 != 0) continue;
                    db.ChangeTracker.DetectChanges();
                    await db.SaveChangesAsync().ConfigureAwait(false);
                    _trace.TraceEvent(TraceEventType.Verbose, 0, "Inserted journal entries: " + count);
                    db.Dispose();
                    db = getDb();
                    db.Configuration.AutoDetectChangesEnabled = false;
                }
                db.ChangeTracker.DetectChanges();
                await db.SaveChangesAsync().ConfigureAwait(false);
            }
        }


        private async Task insertTransactions(IEnumerable<Transaction> transactions) {
            EveProfiteerDbEntities db;
            using (db = getDb()) {
                db.Configuration.AutoDetectChangesEnabled = false;
                IList<Transaction> list = transactions.OrderBy(t => t.TransactionId).ToList();
                int count = 0;
                foreach (Transaction transaction in list) {
                    db.Transactions.Add(transaction);
                    ++count;
                    if (count % 5000 != 0) continue;
                    db.ChangeTracker.DetectChanges();
                    await db.SaveChangesAsync().ConfigureAwait(false);
                    _trace.TraceEvent(TraceEventType.Verbose, 0, "Inserted transactions: " + count);
                    db.Dispose();
                    db = getDb();
                    db.Configuration.AutoDetectChangesEnabled = false;
                }
                db.ChangeTracker.DetectChanges();
                await db.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task UpdateInventoryAsync(IEnumerable<Transaction> transactions) {
            using (var db = getDb()) {
                db.Configuration.AutoDetectChangesEnabled = false;
                List<Asset> assets =
                    await
                        db.Assets.Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id)
                            .ToListAsync()
                            .ConfigureAwait(false);
                Dictionary<int, Asset> assetLookup = assets.ToDictionary(t => t.InvTypes_TypeId, t => t);
                foreach (Transaction transaction in transactions.OrderBy(t => t.TransactionDate)) {
                    Asset asset;
                    if (!assetLookup.TryGetValue(transaction.TypeId, out asset)) {
                        asset = new Asset();
                        asset.InvTypes_TypeId = transaction.TypeId;
                        asset.ApiKeyEntity_Id = transaction.ApiKeyEntity_Id;
                        db.Assets.Add(asset);
                        assetLookup.Add(asset.InvTypes_TypeId, asset);
                    }
                    if (transaction.TransactionType == TransactionType.Buy) {
                        var total = transaction.Price * transaction.Quantity;
                        total *= (decimal)(1 + Properties.Settings.Default.BrokerFeeRate / 100);
                        asset.TotalCost += total;
                        asset.Quantity += transaction.Quantity;
                        asset.LatestAverageCost = asset.TotalCost / asset.Quantity;
                        transaction.PerpetualAverageCost = asset.LatestAverageCost;
                        transaction.CurrentStock = asset.Quantity;
                    } else if (transaction.TransactionType == TransactionType.Sell) {
                        transaction.PerpetualAverageCost = asset.LatestAverageCost;
                        var total = transaction.Quantity * asset.LatestAverageCost;
                        transaction.TaxLiability = total * (decimal)Properties.Settings.Default.TaxRate / 100;
                        transaction.BrokerFee = total * (decimal)Properties.Settings.Default.BrokerFeeRate / 100;
                        if (asset.Quantity > 0) {
                            asset.TotalCost -= total;
                            asset.Quantity -= transaction.Quantity;
                            transaction.CurrentStock = asset.Quantity;
                            if (asset.Quantity <= 0) {
                                transaction.UnaccountedStock += Math.Abs(asset.Quantity);
                                transaction.CurrentStock = 0;
                                asset.UnaccountedQuantity += Math.Abs(asset.Quantity);
                                asset.TotalCost = 0;
                                asset.Quantity = 0;
                            }
                        }
                    }
                }
                db.ChangeTracker.DetectChanges();
                await db.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}