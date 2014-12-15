using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using eZet.EveLib.Modules.Models;
using eZet.EveLib.Modules.Models.Character;
using eZet.EveLib.Modules.Models.Misc;
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
            using (EveProfiteerRepository db = CreateDb()) {
                return
                    await
                        db.Context.ApiKeyEntities.Include(e => e.ApiKeys)
                            .SingleOrDefaultAsync(e => e.Id == id)
                            .ConfigureAwait(false);
            }
        }

        public async Task UpdateIndustryJobs() {
            await
                _eveApiService.GetIndustryJobs(ApplicationHelper.ActiveEntity.ApiKeys.First(),
                    ApplicationHelper.ActiveEntity);
        }

        public async Task UpdateRefIdsAsync() {
            EveApiResponse<ReferenceTypes> reftypes = await _eveApiService.GetRefTypesAsync().ConfigureAwait(false);
            using (EveProfiteerRepository db = CreateDb()) {
                db.Context.RefTypes.RemoveRange(db.Context.RefTypes);
                foreach (ReferenceTypes.ReferenceType apireftype in reftypes.Result.RefTypes) {
                    RefType reftype = db.Context.RefTypes.Create();
                    ApiEntityMapper.Map(apireftype, reftype);
                    db.Context.RefTypes.Add(reftype);
                }
                await db.Context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private async Task<long> getLatestTransactionId() {
            using (EveProfiteerRepository db = CreateDb()) {
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
            using (EveProfiteerRepository db = CreateDb()) {
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

            AssetList result =
                await
                    _eveApiService.GetAssetsAsync(ApplicationHelper.ActiveEntity.ApiKeys.First(),
                        ApplicationHelper.ActiveEntity)
                        .ConfigureAwait(false);
            var typeGrouping =
                result.Items.Where(f => !f.Singleton && f.Flag == 4).GroupBy(asset => asset.TypeId).ToList();
            using (EveProfiteerRepository db = CreateDb()) {
                List<Asset> assets =
                    await
                        db.Context.Assets.Where(asset => asset.ApiKeyEntity_Id == ApplicationHelper.ActiveEntity.Id)
                            .ToListAsync()
                            .ConfigureAwait(false);
                var lookup =
                    assets.ToLookup(asset => Tuple.Create(asset.StationId, asset.InvTypes_TypeId));

                foreach (var typeGroup in typeGrouping) {
                    var locationGrouping = typeGroup.GroupBy(f => (int)f.LocationId);
                    foreach (var locationGroup in locationGrouping) {
                        Asset asset = lookup[Tuple.Create(locationGroup.Key, typeGroup.Key)].SingleOrDefault();
                        if (asset == null) {
                            asset = new Asset {
                                InvTypes_TypeId = typeGroup.Key,
                                ApiKeyEntity_Id = ApplicationHelper.ActiveEntity.Id,
                                StationId = locationGroup.Key
                            };
                            db.Context.Assets.Add(asset);
                        }

                        asset.InventoryQuantity = locationGroup.Sum(item => item.Quantity);
                        //processAssetQuantity(asset);
                        assets.Remove(asset);
                    }
                }
                foreach (Asset asset in assets) {
                    asset.InventoryQuantity = 0;
                }
                _trace.TraceEvent(TraceEventType.Verbose, 0, "Processed {0} assets.", typeGrouping.Count());
                _trace.TraceEvent(TraceEventType.Stop, 0, "CompleteUpdateAssets");
                return await db.Context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        //private static void processAssetQuantity(Asset asset) {
        //    var totalQuantity = asset.ActualQuantity + asset.MarketQuantity;
        //    var diff = totalQuantity - asset.Quantity;
        //    if (totalQuantity == 0) {
        //        asset.MaterialCost = 0;
        //        asset.BrokerFees = 0;
        //    } else if (diff != 0) {
        //        var total = diff * asset.LatestAverageCost;
        //        asset.MaterialCost += total;
        //        asset.BrokerFees += total * (decimal)ApplicationHelper.BrokerFeeRate;
        //        asset.UnaccountedQuantity += diff;
        //    }
        //    asset.Quantity = totalQuantity;
        //}

        public async Task<int> UpdateTransactionsAsync() {
            _trace.TraceEvent(TraceEventType.Start, 0, "StartUpdateTransactions");
            long latest = await getLatestTransactionId().ConfigureAwait(false);
            _trace.TraceEvent(TraceEventType.Verbose, 0, "Latest transaction ID: " + latest);
            List<Transaction> transactions =
                (await _eveApiService.GetNewTransactionsAsync(ApplicationHelper.ActiveEntity.ApiKeys.First(),
                    ApplicationHelper.ActiveEntity, latest).ConfigureAwait(false)).ToList();
            _trace.TraceEvent(TraceEventType.Verbose, 0, "Fetched transactions: " + transactions.Count);
            using (EveProfiteerRepository db = CreateDb()) {
                List<Asset> assets =
                    await
                        db.Context.Assets.Where(asset => asset.ApiKeyEntity_Id == ApplicationHelper.ActiveEntity.Id)
                            .ToListAsync()
                            .ConfigureAwait(false);
                var changedAssets = processTransactionsAsync(transactions, assets);
                foreach (Asset asset in changedAssets) {
                    //transactions.Remove(asset.LastBuyTransaction);
                    //transactions.Remove(asset.LastSellTransaction);
                    if (db.Context.Entry(asset).State == EntityState.Detached) {
                        db.Context.Assets.Add(asset);
                    } else db.Context.Entry(asset).State = EntityState.Modified;
                }
                transactions = db.Context.Transactions.AddRange(transactions).ToList();
                await db.SaveChangesAsync();
            }
            _trace.TraceEvent(TraceEventType.Stop, 0, "CompleteUpdateTransactions");
            return transactions.Count;
        }

        public async Task<int> UpdateJournalAsync() {
            _trace.TraceEvent(TraceEventType.Start, 0, "StartUpdateJournal");
            long latest = await getLatestJournalId();
            _trace.TraceEvent(TraceEventType.Verbose, 0, "Latest journal ID: " + latest);
            IList<JournalEntry> list =
                await
                    _eveApiService.GetNewJournalEntriesAsync(ApplicationHelper.ActiveEntity.ApiKeys.First(),
                        ApplicationHelper.ActiveEntity, latest);
            _trace.TraceEvent(TraceEventType.Verbose, 0, "Fetched journal entries: " + list.Count);
            int result = await insertJournalEntries(list).ConfigureAwait(false);
            _trace.TraceEvent(TraceEventType.Stop, 0, "CompleteUpdateJournal");
            return result;
        }

        public async Task ProcessUnaccountedTransactionsAsync() {
            _trace.TraceEvent(TraceEventType.Start, 0, "StartProcessUnaccountedTransactions");

            _trace.TraceEvent(TraceEventType.Verbose, 0, "Fetching Unaccounted Sales");
            using (EveProfiteerRepository db = CreateDb()) {
                List<Transaction> sell = await db.Context.Transactions.Where(
                    t =>
                        t.ApiKeyEntity_Id == ApplicationHelper.ActiveEntity.Id &&
                        t.TransactionType == TransactionType.Sell && t.PerpetualAverageCost == 0)
                    .ToListAsync()
                    .ConfigureAwait(false);
                _trace.TraceEvent(TraceEventType.Verbose, 0, "Unaccounted sales: " + sell.Count);
                IEnumerable<int> sellIds = sell.Select(t => t.TypeId);
                _trace.TraceEvent(TraceEventType.Verbose, 0, "Fetching Related Buy Transactions");
                List<Transaction> buy = await db.Context.Transactions.AsNoTracking().Where(t =>
                    t.ApiKeyEntity_Id == ApplicationHelper.ActiveEntity.Id && sellIds.Contains(t.TypeId))
                    .GroupBy(t => t.TypeId)
                    .Select(g => g.FirstOrDefault())
                    .ToListAsync()
                    .ConfigureAwait(false);
                _trace.TraceEvent(TraceEventType.Verbose, 0, "Processing Transactions");
                ILookup<int, Transaction> buyLookup = buy.ToLookup(t => t.TypeId);
                foreach (Transaction transaction in sell) {
                    if (buyLookup.Contains(transaction.TypeId))
                        transaction.PerpetualAverageCost = buyLookup[transaction.TypeId].Single().PerpetualAverageCost;
                }
                _trace.TraceEvent(TraceEventType.Verbose, 0, "SaveChangesAsync");

                await db.Context.SaveChangesAsync();
            }
            _trace.TraceEvent(TraceEventType.Stop, 0, "CompleteProcessUnaccountedTransactions");
        }

        public async Task ProcessAllTransactionsAsync() {

            // delete current assets
            _trace.TraceEvent(TraceEventType.Start, 0, "StartProcessAllTransactions");
            using (EveProfiteerRepository db = CreateDb()) {
                db.Context.Configuration.AutoDetectChangesEnabled = false;
                db.Context.Configuration.ValidateOnSaveEnabled = false;
                List<Asset> oldAssets =
                    await
                        db.Context.Assets.Where(asset => asset.ApiKeyEntity_Id == ApplicationHelper.ActiveEntity.Id)
                            .ToListAsync()
                            .ConfigureAwait(false);
                db.Context.Assets.RemoveRange(oldAssets);
                oldAssets.ForEach(e => db.Context.Entry(e).State = EntityState.Deleted);
                _trace.TraceEvent(TraceEventType.Verbose, 0, "Deleting Assets: " + oldAssets.Count);
                await db.Context.SaveChangesAsync().ConfigureAwait(false);
            }

            // reset and reprocess transactions in batches
            int batch = 0;
            var assets = new List<Asset>();
            while (true) {
                using (EveProfiteerRepository db = CreateDb()) {
                    db.Context.Configuration.AutoDetectChangesEnabled = false;
                    db.Context.Configuration.ValidateOnSaveEnabled = false;
                    IOrderedQueryable<Transaction> query =
                        db.Context.Transactions.AsNoTracking()
                            .Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveEntity.Id)
                            .OrderBy(e => e.TransactionDate);
                    List<Transaction> transactions =
                        await query.Skip(batch * 5000).Take(5000).ToListAsync().ConfigureAwait(false);
                    if (!transactions.Any()) break;
                    foreach (Transaction transaction in transactions) {
                        transaction.PerpetualAverageCost = 0;
                        transaction.UnaccountedQuantity = 0;
                        transaction.TaxLiability = 0;
                        transaction.CogsBrokerFees = 0;
                        transaction.CogsMaterialCost = 0;
                        transaction.PostTransactionStock = 0;
                        db.Context.Entry(transaction).State = EntityState.Modified;
                    }
                    _trace.TraceEvent(TraceEventType.Verbose, 0, "Processing batch: {0}", batch);
                    //transactions.ForEach(e => db.Context.Entry(e).State = EntityState.Modified);
                    processTransactionsAsync(transactions, assets);
                    _trace.TraceEvent(TraceEventType.Verbose, 0, "Saving: {0}", transactions.Count());
                    await db.Context.SaveChangesAsync().ConfigureAwait(false);
                    ++batch;
                }
            }

            // save assets
            using (EveProfiteerRepository db = CreateDb()) {
                foreach (Asset asset in assets) {
                    if (asset.LastBuyTransaction != null) {
                        asset.LastBuyTransactionId = asset.LastBuyTransaction.Id;
                        asset.LastBuyTransaction = null;
                    }
                    if (asset.LastSellTransaction != null) {
                        asset.LastSellTransactionId = asset.LastSellTransaction.Id;
                        asset.LastSellTransaction = null;
                    }
                    db.Context.Assets.Add(asset);
                }
                _trace.TraceEvent(TraceEventType.Verbose, 0, "Saving Assets: {0}", assets.Count());
                await db.SaveChangesAsync().ConfigureAwait(false);
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
                db.Context.Configuration.ValidateOnSaveEnabled = false;
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
                    db.Context.Configuration.ValidateOnSaveEnabled = false;
                }
                db.Context.ChangeTracker.DetectChanges();
                result = await db.Context.SaveChangesAsync().ConfigureAwait(false);
            }
            _trace.TraceEvent(TraceEventType.Stop, 0, "CompleteInsertTransactions");
            return result;
        }

        public List<Asset> processTransactionsAsync(IList<Transaction> transactions, List<Asset> assets) {
            _trace.TraceEvent(TraceEventType.Start, 0, "StartProcessTransactions");
            var changedAssets = new HashSet<Asset>();

            Dictionary<int, Asset> assetLookup = assets.ToDictionary(t => t.InvTypes_TypeId, t => t);
            foreach (Transaction transaction in transactions.OrderBy(t => t.TransactionDate)) {
                Asset asset;
                if (!assetLookup.TryGetValue(transaction.TypeId, out asset)) {
                    asset = new Asset {
                        InvTypes_TypeId = transaction.TypeId,
                        ApiKeyEntity_Id = transaction.ApiKeyEntity_Id
                    };
                    assetLookup.Add(asset.InvTypes_TypeId, asset);
                    assets.Add(asset);
                }
                if (!changedAssets.Contains(asset)) changedAssets.Add(asset);
                // Moving average cost
                decimal transactionTotal = transaction.Quantity * transaction.Price;
                transaction.BrokerFee = transactionTotal * (decimal)ApplicationHelper.BrokerFeeRate / 100;
                if (transaction.TransactionType == TransactionType.Buy) {
                    if (asset.LastBuyTransaction == null ||
                        transaction.TransactionDate > asset.LastBuyTransaction.TransactionDate) {
                        asset.LastBuyTransaction = transaction;
                    }
                    asset.MaterialCost += transactionTotal;
                    asset.BrokerFees += transaction.BrokerFee;
                    asset.CalculatedQuantity += transaction.Quantity;
                    asset.LatestAverageCost = (asset.MaterialCost + asset.BrokerFees) / asset.CalculatedQuantity;
                    transaction.PerpetualAverageCost = (asset.MaterialCost + asset.BrokerFees) / asset.CalculatedQuantity;
                    transaction.PostTransactionStock = asset.CalculatedQuantity;
                } else if (transaction.TransactionType == TransactionType.Sell) {
                    if (asset.LastSellTransaction == null ||
                        transaction.TransactionDate > asset.LastSellTransaction.TransactionDate) {
                        asset.LastSellTransaction = transaction;
                    }
                    transaction.TaxLiability = transactionTotal * (decimal)ApplicationHelper.TaxRate / 100;
                    if (asset.CalculatedQuantity > 0) {
                        // If we have item in stock, specify costs
                        transaction.PerpetualAverageCost = (asset.MaterialCost + asset.BrokerFees) /
                                                           asset.CalculatedQuantity;
                        transaction.CogsBrokerFees = transaction.Quantity * (asset.BrokerFees / asset.CalculatedQuantity);
                        transaction.CogsMaterialCost = transaction.Quantity *
                                                       (asset.MaterialCost / asset.CalculatedQuantity);
                        asset.MaterialCost -= transaction.Quantity * (asset.MaterialCost / asset.CalculatedQuantity);
                        asset.BrokerFees -= transaction.Quantity * (asset.BrokerFees / asset.CalculatedQuantity);
                    }
                    asset.CalculatedQuantity -= transaction.Quantity;
                    if (asset.CalculatedQuantity <= 0) {
                        transaction.UnaccountedQuantity = Math.Abs(asset.CalculatedQuantity);
                        asset.UnaccountedQuantity -= transaction.UnaccountedQuantity;
                        asset.MaterialCost = 0;
                        asset.BrokerFees = 0;
                        asset.CalculatedQuantity = 0;
                        asset.LatestAverageCost = 0;
                    }
                    transaction.PostTransactionStock = asset.CalculatedQuantity;
                }
            }

            _trace.TraceEvent(TraceEventType.Stop, 0, "CompleteProcessTransactions");
            return changedAssets.ToList();
        }


        public async Task<int> UpdateMarketOrdersAsync() {
            MarketOrders result =
                await
                    _eveApiService.GetMarketOrdersAsync(ApplicationHelper.ActiveEntity.ApiKeys.First(),
                        ApplicationHelper.ActiveEntity)
                        .ConfigureAwait(false);
            long minOrderId = result.Orders.Any() ? result.Orders.MinBy(order => order.OrderId).OrderId : 0;
            using (EveProfiteerRepository db = CreateDb()) {
                db.Context.Configuration.ValidateOnSaveEnabled = false;
                db.Context.Configuration.AutoDetectChangesEnabled = false;
                List<MarketOrders.MarketOrder> newOrders = result.Orders.ToList();
                List<MarketOrder> orders =
                    await
                        db.MyMarketOrders()
                            .Where(order => order.OrderId >= minOrderId || order.OrderState == OrderState.Open)
                            .ToListAsync()
                            .ConfigureAwait(false);
                ILookup<long, MarketOrders.MarketOrder> resultLookup = result.Orders.ToLookup(f => f.OrderId);
                foreach (MarketOrder order in orders) {
                    MarketOrders.MarketOrder marketOrder = resultLookup[order.OrderId].SingleOrDefault();
                    if (marketOrder != null) {
                        ApiEntityMapper.Map(marketOrder, order);
                        newOrders.Remove(marketOrder);
                    } else if (order.OrderState == OrderState.Open) {
                        order.Escrow = 0;
                        if (order.Duration < DateTime.UtcNow.Subtract(order.Issued).Days) {
                            order.OrderState = OrderState.Expired;
                        } else {
                            order.OrderState = OrderState.Closed;
                        }
                    }
                }
                foreach (MarketOrders.MarketOrder marketOrder in newOrders) {
                    MarketOrder order = db.Context.MarketOrders.Create();
                    ApiEntityMapper.Map(marketOrder, order);
                    db.Context.MarketOrders.Add(order);
                }


                List<Asset> assets = await db.Context.Assets.ToListAsync().ConfigureAwait(false);
                foreach (Asset asset in assets) {
                    List<MarketOrders.MarketOrder> order =
                        result.Orders.Where(
                            e => e.TypeId == asset.InvTypes_TypeId && e.StationId == asset.StationId && e.OrderState == 0 && e.Bid == 0 && e.VolumeRemaining != 0)
                            .ToList();
                    asset.MarketQuantity = order.Any() ? order.Sum(e => e.VolumeRemaining) : 0;
                }
                db.Context.ChangeTracker.DetectChanges();
                return await db.Context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<ApiKeyEntity>> GetAllActiveEntities() {
            using (EveProfiteerRepository db = CreateDb()) {
                return await db.Context.ApiKeyEntities.Include(e => e.ApiKeys).ToListAsync().ConfigureAwait(false);
            }
        }
    }
}