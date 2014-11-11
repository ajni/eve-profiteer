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

        public async Task<ApiKeyEntity> FindApiEntity(int id) {
            using (var db = CreateDb()) {
                return await db.Context.ApiKeyEntities.Include(e => e.ApiKeys).SingleOrDefaultAsync(e => e.Id == id).ConfigureAwait(false);
            }
        }


        public async Task UpdateIndustryJobs() {
            await _eveApiService.GetIndustryJobs(ApplicationHelper.ActiveEntity.ApiKeys.First(), ApplicationHelper.ActiveEntity);
        }

        public async Task UpdateRefIdsAsync() {
            var reftypes = await _eveApiService.GetRefTypesAsync().ConfigureAwait(false);
            using (var db = CreateDb()) {
                db.Context.RefTypes.RemoveRange(db.Context.RefTypes);
                foreach (var apireftype in reftypes.Result.RefTypes) {
                    var reftype = db.Context.RefTypes.Create();
                    ApiEntityMapper.Map(apireftype, reftype);
                    db.Context.RefTypes.Add(reftype);
                }
                await db.Context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private async Task<long> getLatestTransactionId() {
            using (var db = CreateDb()) {
                return
                    await
                        db.Context.Transactions.AsNoTracking()
                            .Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveEntity.Id)
                            .OrderByDescending(t => t.TransactionId)
                            .Select(t => t.TransactionId)
                            .FirstOrDefaultAsync().ConfigureAwait(false);
            }
        }

        private async Task<long> getLatestJournalId() {
            using (var db = CreateDb()) {
                return
                    await
                        db.Context.JournalEntries.AsNoTracking()
                            .Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveEntity.Id)
                            .OrderByDescending(t => t.RefId)
                            .Select(t => t.RefId)
                            .FirstOrDefaultAsync().ConfigureAwait(false);
            }
        }

        public async Task<int> UpdateAssetsAsync() {
            _trace.TraceEvent(TraceEventType.Start, 0, "StartUpdateAssets");

            var result =
                await
                    _eveApiService.GetAssetsAsync(ApplicationHelper.ActiveEntity.ApiKeys.First(), ApplicationHelper.ActiveEntity)
                        .ConfigureAwait(false);
            var groups = result.Flatten().Where(asset => !asset.Singleton).GroupBy(asset => asset.TypeId).ToList();
            using (var db = CreateDb()) {
                var assets =
                    await
                        db.Context.Assets.Where(asset => asset.ApiKeyEntity_Id == ApplicationHelper.ActiveEntity.Id)
                            .ToListAsync()
                            .ConfigureAwait(false);
                var lookup = assets.ToLookup(asset => asset.InvTypes_TypeId);
                foreach (var group in groups) {
                    var asset = lookup[group.Key].SingleOrDefault();
                    if (asset == null) {
                        asset = new Asset();
                        asset.InvTypes_TypeId = group.Key;
                        asset.ApiKeyEntity_Id = ApplicationHelper.ActiveEntity.Id;
                        db.Context.Assets.Add(asset);
                    }
                    asset.ActualQuantity = group.Sum(item => item.Quantity);
                    //processAssetQuantity(asset);
                    assets.Remove(asset);
                }
                foreach (var asset in assets) {
                    asset.ActualQuantity = 0;
                }
                _trace.TraceEvent(TraceEventType.Verbose, 0, "Processed {0} assets.", groups.Count());
                _trace.TraceEvent(TraceEventType.Stop, 0, "CompleteUpdateAssets");
                return await db.Context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private static void processAssetQuantity(Asset asset) {
            var totalQuantity = asset.ActualQuantity + asset.MarketQuantity;
            var diff = totalQuantity - asset.Quantity;
            if (totalQuantity == 0) {
                asset.MaterialCost = 0;
                asset.BrokerFees = 0;
            } else if (diff != 0) {
                var total = diff * asset.LatestAverageCost;
                asset.MaterialCost += total;
                asset.BrokerFees += total * (decimal)ApplicationHelper.BrokerFeeRate;
                asset.UnaccountedQuantity += diff;
            }
            asset.Quantity = totalQuantity;
        }

        public async Task<int> UpdateTransactionsAsync() {
            _trace.TraceEvent(TraceEventType.Start, 0, "StartUpdateTransactions");
            long latest = await getLatestTransactionId().ConfigureAwait(false);
            _trace.TraceEvent(TraceEventType.Verbose, 0, "Latest transaction ID: " + latest);
            var transactions =
                await
                    Task.Run(
                        () =>
                            _eveApiService.GetNewTransactionsAsync(ApplicationHelper.ActiveEntity.ApiKeys.First(),
                                ApplicationHelper.ActiveEntity, latest));
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
                    _eveApiService.GetNewJournalEntriesAsync(ApplicationHelper.ActiveEntity.ApiKeys.First(),
                        ApplicationHelper.ActiveEntity, latest);
            _trace.TraceEvent(TraceEventType.Verbose, 0, "Fetched journal entries: " + list.Count);
            var result = await insertJournalEntries(list).ConfigureAwait(false);
            _trace.TraceEvent(TraceEventType.Stop, 0, "CompleteUpdateJournal");
            return result;
        }

        public async Task ProcessUnaccountedTransactionsAsync() {
            _trace.TraceEvent(TraceEventType.Start, 0, "StartProcessUnaccountedTransactions");

            _trace.TraceEvent(TraceEventType.Verbose, 0, "Fetching Unaccounted Sales");
            using (var db = CreateDb()) {
                var sell = await db.Context.Transactions.Where(
                    t =>
                        t.ApiKeyEntity_Id == ApplicationHelper.ActiveEntity.Id &&
                        t.TransactionType == TransactionType.Sell && t.PerpetualAverageCost == 0)
                    .ToListAsync()
                    .ConfigureAwait(false);
                _trace.TraceEvent(TraceEventType.Verbose, 0, "Unaccounted sales: " + sell.Count);
                var sellIds = sell.Select(t => t.TypeId);
                _trace.TraceEvent(TraceEventType.Verbose, 0, "Fetching Related Buy Transactions");
                var buy = await db.Context.Transactions.AsNoTracking().Where(t =>
                    t.ApiKeyEntity_Id == ApplicationHelper.ActiveEntity.Id && sellIds.Contains(t.TypeId)).GroupBy(t => t.TypeId).Select(g => g.FirstOrDefault())
                    .ToListAsync()
                    .ConfigureAwait(false);
                _trace.TraceEvent(TraceEventType.Verbose, 0, "Processing Transactions");
                var buyLookup = buy.ToLookup(t => t.TypeId);
                foreach (var transaction in sell) {
                    if (buyLookup.Contains(transaction.TypeId))
                        transaction.PerpetualAverageCost = buyLookup[transaction.TypeId].Single().PerpetualAverageCost;
                }
                _trace.TraceEvent(TraceEventType.Verbose, 0, "SaveChangesAsync");

                await db.Context.SaveChangesAsync();
            }
            _trace.TraceEvent(TraceEventType.Stop, 0, "CompleteProcessUnaccountedTransactions");
        }

        public async Task ProcessAllTransactionsAsync() {
            _trace.TraceEvent(TraceEventType.Start, 0, "StartProcessAllTransactions");
            using (var db = CreateDb()) {
                db.Context.Configuration.AutoDetectChangesEnabled = false;
                db.Context.Configuration.ValidateOnSaveEnabled = false;
                var assets =
                    await
                        db.Context.Assets.Where(asset => asset.ApiKeyEntity_Id == ApplicationHelper.ActiveEntity.Id)
                            .ToListAsync()
                            .ConfigureAwait(false);
                db.Context.Assets.RemoveRange(assets);
                assets.ForEach(e => db.Context.Entry(e).State = EntityState.Deleted);
                _trace.TraceEvent(TraceEventType.Verbose, 0, "Deleting Assets: " + assets.Count);
                await db.Context.SaveChangesAsync().ConfigureAwait(false);
            }

            int batch = 0;
            while (true) {
                using (var db = CreateDb()) {
                    db.Context.Configuration.AutoDetectChangesEnabled = false;
                    db.Context.Configuration.ValidateOnSaveEnabled = false;
                    IOrderedQueryable<Transaction> query = db.Context.Transactions.AsNoTracking().Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveEntity.Id).OrderBy(e => e.TransactionDate);
                    List<Transaction> transactions = await query.Skip(batch * 1000).Take(1000).ToListAsync().ConfigureAwait(false);
                    if (!transactions.Any()) break;
                    transactions.Apply(t => {
                        t.PerpetualAverageCost = 0;
                        t.UnaccountedQuantity = 0;
                        t.TaxLiability = 0;
                        t.CogsBrokerFees = 0;
                        t.CogsMaterialCost = 0;
                        t.PostTransactionStock = 0;
                        db.Context.Entry(t).State = EntityState.Modified;
                    });
                    _trace.TraceEvent(TraceEventType.Verbose, 0, "Processing batch: {0}", batch);
                    //transactions.ForEach(e => db.Context.Entry(e).State = EntityState.Modified);
                    await processTransactionsAsync(transactions).ConfigureAwait(false);
                    _trace.TraceEvent(TraceEventType.Verbose, 0, "Saving: {0}", transactions.Count());
                    await db.Context.SaveChangesAsync().ConfigureAwait(false);
                    ++batch;
                }
            }
            _trace.TraceEvent(TraceEventType.Stop, 0, "CompleteProcessAllTransactions");
        }

        private async Task<int> insertJournalEntries(IEnumerable<JournalEntry> entries) {
            _trace.TraceEvent(TraceEventType.Start, 0, "StartInsertJournalEntries");
            int result;
            EveProfiteerRepository db;
            using (db = CreateDb()) {
                db.Context.Configuration.AutoDetectChangesEnabled = false;
                IList<JournalEntry> list = entries.OrderBy(entry => entry.RefId).ToList();
                int count = 0;
                foreach (JournalEntry entry in list) {
                    db.Context.JournalEntries.Add(entry);
                    ++count;
                    if (count % 5000 != 0) continue;
                    db.Context.ChangeTracker.DetectChanges();
                    await db.Context.SaveChangesAsync().ConfigureAwait(false);
                    _trace.TraceEvent(TraceEventType.Verbose, 0, "Inserted journal entries: " + count);
                    db.Context.Dispose();
                    db = CreateDb();
                    db.Context.Configuration.AutoDetectChangesEnabled = false;
                }
                db.Context.ChangeTracker.DetectChanges();
                result = await db.Context.SaveChangesAsync().ConfigureAwait(false);
            }
            _trace.TraceEvent(TraceEventType.Stop, 0, "CompleteInsertJournalEntries");
            return result;
        }


        private async Task<int> insertTransactions(IEnumerable<Transaction> transactions) {
            _trace.TraceEvent(TraceEventType.Start, 0, "StartInsertTransactions");
            int result;
            EveProfiteerRepository db;
            using (db = CreateDb()) {
                db.Context.Configuration.AutoDetectChangesEnabled = false;
                IList<Transaction> list = transactions.OrderBy(t => t.TransactionId).ToList();
                int count = 0;
                foreach (Transaction transaction in list) {
                    db.Context.Transactions.Add(transaction);
                    ++count;
                    if (count % 5000 != 0) continue;
                    db.Context.ChangeTracker.DetectChanges();
                    await db.Context.SaveChangesAsync().ConfigureAwait(false);
                    _trace.TraceEvent(TraceEventType.Verbose, 0, "Inserted transactions: " + count);
                    db.Context.Dispose();
                    db = CreateDb();
                    db.Context.Configuration.AutoDetectChangesEnabled = false;
                }
                db.Context.ChangeTracker.DetectChanges();
                result = await db.Context.SaveChangesAsync().ConfigureAwait(false);
            }
            _trace.TraceEvent(TraceEventType.Stop, 0, "CompleteInsertTransactions");
            return result;
        }

        public async Task processTransactionsAsync(IEnumerable<Transaction> transactions) {
            _trace.TraceEvent(TraceEventType.Start, 0, "StartProcessTransactions");

            using (var db = CreateDb()) {
                db.Context.Configuration.AutoDetectChangesEnabled = false;
                db.Context.Configuration.ValidateOnSaveEnabled = false;
                List<Asset> assets =
                    await
                        db.Context.Assets.Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveEntity.Id)
                            .ToListAsync()
                            .ConfigureAwait(false);
                Dictionary<int, Asset> assetLookup = assets.ToDictionary(t => t.InvTypes_TypeId, t => t);
                foreach (Transaction transaction in transactions.OrderBy(t => t.TransactionDate)) {
                    Asset asset;
                    if (!assetLookup.TryGetValue(transaction.TypeId, out asset)) {
                        asset = new Asset();
                        asset.InvTypes_TypeId = transaction.TypeId;
                        asset.ApiKeyEntity_Id = transaction.ApiKeyEntity_Id;
                        db.Context.Assets.Add(asset);
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
                            asset.UnaccountedQuantity -= transaction.UnaccountedQuantity;
                            asset.MaterialCost = 0;
                            asset.BrokerFees = 0;
                            asset.Quantity = 0;
                        }
                        //processAssetQuantity(asset);

                        transaction.PostTransactionStock = asset.Quantity;
                    }
                }
                // save changes to assets
                db.Context.ChangeTracker.DetectChanges();
                await db.Context.SaveChangesAsync().ConfigureAwait(false);
            }
            _trace.TraceEvent(TraceEventType.Stop, 0, "CompleteProcessTransactions");
        }

        public async Task<int> UpdateMarketOrdersAsync() {
            var result = await _eveApiService.GetMarketOrdersAsync(ApplicationHelper.ActiveEntity.ApiKeys.First(), ApplicationHelper.ActiveEntity)
                .ConfigureAwait(false);
            var minOrderId = result.Orders.MinBy(order => order.OrderId).OrderId;
            using (var db = CreateDb()) {
                db.Context.Configuration.ValidateOnSaveEnabled = false;
                db.Context.Configuration.AutoDetectChangesEnabled = false;
                var newOrders = result.Orders.ToList();
                var orders = await db.MyMarketOrders().Where(order => order.OrderId >= minOrderId || order.OrderState == OrderState.Open).ToListAsync().ConfigureAwait(false);
                var resultLookup = result.Orders.ToLookup(f => f.OrderId);
                foreach (var order in orders) {
                    var marketOrder = resultLookup[order.OrderId].SingleOrDefault();
                    if (marketOrder != null) {
                        ApiEntityMapper.Map(marketOrder, order);
                        newOrders.Remove(marketOrder);
                    }
                    else if (order.OrderState == OrderState.Open) {
                        order.Escrow = 0;
                        if (order.Duration < DateTime.UtcNow.Subtract(order.Issued).Days) {
                            order.OrderState = OrderState.Expired;
                        } else {
                            order.OrderState = OrderState.Closed;
                        }
                    }
                }
                foreach (var marketOrder in newOrders) {
                    var order = db.Context.MarketOrders.Create();
                    ApiEntityMapper.Map(marketOrder, order);
                    db.Context.MarketOrders.Add(order);
                }


                var assets = await db.Context.Assets.ToListAsync().ConfigureAwait(false);
                foreach (var asset in assets) {
                    var order = result.Orders.Where(e => e.TypeId == asset.InvTypes_TypeId && e.OrderState == 0 && e.Bid == 0 && e.VolumeRemaining != 0);
                    asset.MarketQuantity = order.Any() ? order.Sum(e => e.VolumeRemaining) : 0;
                    //processAssetQuantity(asset);
                }
                db.Context.ChangeTracker.DetectChanges();
                return await db.Context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<ApiKeyEntity>>  GetAllActiveEntities() {
            using (var db = CreateDb()) {
                return await db.Context.ApiKeyEntities.Include(e => e.ApiKeys).ToListAsync().ConfigureAwait(false);
            }
        }
    }
}