using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;
using MoreLinq;

namespace eZet.EveProfiteer.Services {
    public class ShellService : DbContextService {
        private readonly EveApiService _eveApiService;

        private readonly TraceSource _trace = new TraceSource("EveProfiteer", SourceLevels.All);


        public ShellService(EveApiService eveApiService) {
            _eveApiService = eveApiService;
        }


        public async Task UpdateIndustryJobs() {
            await _eveApiService.GetIndustryJobs(ApplicationHelper.ActiveKey, ApplicationHelper.ActiveKeyEntity);
        }

        public async Task UpdateRefIdsAsync() {
            var reftypes = await _eveApiService.GetRefTypesAsync().ConfigureAwait(false);
            using (var db = CreateDb()) {
                db.RefTypes.RemoveRange(db.RefTypes);
                foreach (var apireftype in reftypes.Result.RefTypes) {
                    var reftype = db.RefTypes.Create();
                    ApiEntityMapper.Map(apireftype, reftype);
                    db.RefTypes.Add(reftype);
                }
                await db.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private async Task<long> getLatestTransactionId() {
            using (EveProfiteerDbEntities db = CreateDb()) {
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
            using (EveProfiteerDbEntities db = CreateDb()) {
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
            _trace.TraceEvent(TraceEventType.Start, 0, "StartUpdateAssets");

            var result =
                await
                    _eveApiService.GetAssetsAsync(ApplicationHelper.ActiveKey, ApplicationHelper.ActiveKeyEntity)
                        .ConfigureAwait(false);
            var groups = result.Flatten().Where(asset => !asset.Singleton).GroupBy(asset => asset.TypeId).ToList();
            using (var db = CreateDb()) {
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
                _trace.TraceEvent(TraceEventType.Stop, 0, "CompleteUpdateAssets");
                return await db.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task<int> UpdateTransactionsAsync() {
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
            await processTransactionsAsync(transactions).ConfigureAwait(false);
            var result = await insertTransactions(transactions).ConfigureAwait(false);
            _trace.TraceEvent(TraceEventType.Stop, 0, "CompleteUpdateTransactions");
            return result;
        }

        public async Task<int> UpdateJournalAsync() {
            _trace.TraceEvent(TraceEventType.Start, 0, "StartUpdateJournal");
            long latest = await getLatestJournalId();
            _trace.TraceEvent(TraceEventType.Verbose, 0, "Latest journal ID: " + latest);
            var list =
                await
                    _eveApiService.GetNewJournalEntriesAsync(ApplicationHelper.ActiveKey,
                        ApplicationHelper.ActiveKeyEntity, latest);
            _trace.TraceEvent(TraceEventType.Verbose, 0, "Fetched journal entries: " + list.Count);
            var result = await insertJournalEntries(list).ConfigureAwait(false);
            _trace.TraceEvent(TraceEventType.Stop, 0, "CompleteUpdateJournal");
            return result;
        }

        public async Task ProcessUnaccountedTransactionsAsync() {
            _trace.TraceEvent(TraceEventType.Start, 0, "StartProcessUnaccountedTransactions");

            _trace.TraceEvent(TraceEventType.Verbose, 0, "Fetching Unaccounted Sales");
            using (var db = CreateDb()) {
                var sell = await db.Transactions.Where(
                    t =>
                        t.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id &&
                        t.TransactionType == TransactionType.Sell && t.PerpetualAverageCost == 0)
                    .ToListAsync()
                    .ConfigureAwait(false);
                _trace.TraceEvent(TraceEventType.Verbose, 0, "Unaccounted sales: " + sell.Count);
                var sellIds = sell.Select(t => t.TypeId);
                _trace.TraceEvent(TraceEventType.Verbose, 0, "Fetching Related Buy Transactions");
                var buy = await db.Transactions.AsNoTracking().Where(t =>
                    t.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id && sellIds.Contains(t.TypeId)).GroupBy(t => t.TypeId).Select(g => g.FirstOrDefault())
                    .ToListAsync()
                    .ConfigureAwait(false);
                _trace.TraceEvent(TraceEventType.Verbose, 0, "Processing Transactions");
                var buyLookup = buy.ToLookup(t => t.TypeId);
                foreach (var transaction in sell) {
                    if (buyLookup.Contains(transaction.TypeId))
                        transaction.PerpetualAverageCost = buyLookup[transaction.TypeId].Single().PerpetualAverageCost;
                }
                _trace.TraceEvent(TraceEventType.Verbose, 0, "SaveChangesAsync");

                await db.SaveChangesAsync();
            }
            _trace.TraceEvent(TraceEventType.Stop, 0, "CompleteProcessUnaccountedTransactions");
        }

        public async Task ProcessAllTransactionsAsync() {
            _trace.TraceEvent(TraceEventType.Start, 0, "StartProcessAllTransactions");
            using (var db = CreateDb()) {
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ValidateOnSaveEnabled = false;
                var assets = await db.Assets.Where(asset => asset.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id).ToListAsync().ConfigureAwait(false);
                db.Assets.RemoveRange(assets);
                _trace.TraceEvent(TraceEventType.Verbose, 0, "Deleting Assets: " + assets.Count);
                await db.SaveChangesAsync().ConfigureAwait(false);
                IList<Transaction> transactions = await db.Transactions.Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id).ToListAsync().ConfigureAwait(false);
                _trace.TraceEvent(TraceEventType.Verbose, 0, "Processing Transactions");
                transactions.Apply(t => {
                    t.PerpetualAverageCost = 0;
                    t.UnaccountedQuantity = 0;
                });
                await processTransactionsAsync(transactions);
                _trace.TraceEvent(TraceEventType.Verbose, 0, "Saving Transactions: {0}", transactions.Count);
                db.ChangeTracker.DetectChanges();
                await db.SaveChangesAsync().ConfigureAwait(false);
            }
            _trace.TraceEvent(TraceEventType.Stop, 0, "CompleteProcessAllTransactions");
        }

        private async Task<int> insertJournalEntries(IEnumerable<JournalEntry> entries) {
            _trace.TraceEvent(TraceEventType.Start, 0, "StartInsertJournalEntries");
            EveProfiteerDbEntities db;
            int result;
            using (db = CreateDb()) {
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
                    db = CreateDb();
                    db.Configuration.AutoDetectChangesEnabled = false;
                }
                db.ChangeTracker.DetectChanges();
                result = await db.SaveChangesAsync().ConfigureAwait(false);
            }
            _trace.TraceEvent(TraceEventType.Stop, 0, "CompleteInsertJournalEntries");
            return result;
        }


        private async Task<int> insertTransactions(IEnumerable<Transaction> transactions) {
            _trace.TraceEvent(TraceEventType.Start, 0, "StartInsertTransactions");
            EveProfiteerDbEntities db;
            int result;
            using (db = CreateDb()) {
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
                    db = CreateDb();
                    db.Configuration.AutoDetectChangesEnabled = false;
                }
                db.ChangeTracker.DetectChanges();
                result = await db.SaveChangesAsync().ConfigureAwait(false);
            }
            _trace.TraceEvent(TraceEventType.Stop, 0, "CompleteInsertTransactions");
            return result;
        }

        public async Task processTransactionsAsync(IEnumerable<Transaction> transactions) {
            _trace.TraceEvent(TraceEventType.Start, 0, "StartProcessTransactions");

            using (var db = CreateDb()) {
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
                    // Moving average cost
                    var transactionTotal = transaction.Quantity * transaction.Price;
                    transaction.BrokerFee = transactionTotal * (decimal)ApplicationHelper.BrokerFeeRate / 100;
                    if (transaction.TransactionType == TransactionType.Buy) {
                        asset.MaterialCost += transactionTotal;
                        asset.BrokerFees += transaction.BrokerFee;
                        asset.Quantity += transaction.Quantity;
                        asset.LatestAverageCost = (asset.MaterialCost + asset.BrokerFees) / asset.Quantity;
                        transaction.PerpetualAverageCost = asset.LatestAverageCost;
                        transaction.PostTransactionStock = asset.Quantity;
                    } else if (transaction.TransactionType == TransactionType.Sell) {
                        transaction.TaxLiability = transactionTotal * (decimal)ApplicationHelper.TaxRate / 100;
                        transaction.PerpetualAverageCost = asset.LatestAverageCost;
                        if (asset.Quantity > 0) {
                            // If we have item in stock, specify costs
                            transaction.CogsBrokerFees = transaction.Quantity * (asset.BrokerFees / asset.Quantity);
                            transaction.CogsMaterialCost = transaction.Quantity * (asset.MaterialCost / asset.Quantity);
                            asset.MaterialCost -= transaction.Quantity * (asset.MaterialCost / asset.Quantity);
                            asset.BrokerFees -= transaction.Quantity * (asset.BrokerFees / asset.Quantity);
                        }
                        asset.Quantity -= transaction.Quantity;
                        if (asset.Quantity <= 0) {
                            transaction.UnaccountedQuantity = Math.Abs(asset.Quantity);
                            asset.UnaccountedQuantity += Math.Abs(asset.Quantity);
                            asset.MaterialCost = 0;
                            asset.BrokerFees = 0;
                            asset.Quantity = 0;
                        }
                        transaction.PostTransactionStock = asset.Quantity;
                    }
                }
                db.ChangeTracker.DetectChanges();
                await db.SaveChangesAsync().ConfigureAwait(false);
            }
            _trace.TraceEvent(TraceEventType.Stop, 0, "CompleteProcessTransactions");
        }

        public async Task<int> UpdateMarketOrdersAsync() {
            var result = await _eveApiService.GetMarketOrdersAsync(ApplicationHelper.ActiveKey, ApplicationHelper.ActiveKeyEntity)
                .ConfigureAwait(false);
            var minOrderId = result.Orders.MinBy(order => order.OrderId).OrderId;
            using (var db = CreateDb()) {
                var orders = await
                    MyMarketOrders(db).Where(order => order.OrderId >= minOrderId).ToListAsync().ConfigureAwait(false);
                var orderLookup = orders.ToLookup(f => f.OrderId);
                foreach (var order in result.Orders) {
                    var marketOrder = orderLookup[order.OrderId].SingleOrDefault();
                    if (marketOrder == null) {
                        marketOrder = db.MarketOrders.Create();
                        db.MarketOrders.Add(marketOrder);
                    }
                    ApiEntityMapper.Map(order, marketOrder);
                }
                return await db.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}