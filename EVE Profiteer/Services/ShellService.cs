using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using eZet.EveProfiteer.Events;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;

namespace eZet.EveProfiteer.Services {
    public class ShellService {
        private readonly EveApiService _eveApiService;
        private readonly IEventAggregator _eventAggregator;

        private readonly TraceSource _trace = new TraceSource("EveProfiteer", SourceLevels.All);


        public ShellService(IEventAggregator eventAggregator, EveApiService eveApiService) {
            _eventAggregator = eventAggregator;
            _eveApiService = eveApiService;
        }


        public EveProfiteerDbEntities GetDb() {
            return IoC.Get<EveProfiteerDbEntities>();
        }

        private async Task<long> getLatestTransactionId() {
            using (EveProfiteerDbEntities db = GetDb()) {
                return await db.Transactions.AsNoTracking().Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id)
                    .OrderByDescending(t => t.TransactionId)
                    .Select(t => t.TransactionId)
                    .FirstOrDefaultAsync().ConfigureAwait(false);
            }
        }

        private async Task<long> getLatestJournalId() {
            using (EveProfiteerDbEntities db = GetDb()) {
                return await db.JournalEntries.AsNoTracking().Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id)
                    .OrderByDescending(t => t.RefId)
                    .Select(t => t.RefId)
                    .FirstOrDefaultAsync().ConfigureAwait(false);
            }
        }

        public async Task UpdateTransactions() {
            _trace.TraceEvent(TraceEventType.Start, 0, "StartUpdateTransactions");
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Fetching new transactions..."));
            long latest = await getLatestTransactionId().ConfigureAwait(false);
            _trace.TraceEvent(TraceEventType.Verbose, 0, "Latest transaction ID: " + latest);
            var transactions = await Task.Run(() => _eveApiService.GetNewTransactionsAsync(ApplicationHelper.ActiveKey, ApplicationHelper.ActiveKeyEntity, latest));
            _trace.TraceEvent(TraceEventType.Verbose, 0, "Fetched transactions: " + transactions.Count);
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Processing transactions..."));
            await updateInventory(transactions).ConfigureAwait(false);
            await insertTransactions(transactions).ConfigureAwait(false);
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Transaction update complete"));
            _trace.TraceEvent(TraceEventType.Stop, 0, "CompleteUpdateTransactions");
        }

        public async Task UpdateJournal() {
            _trace.TraceEvent(TraceEventType.Start, 0, "StartUpdateJournal");
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Fetching new journal entries..."));
            long latest = await getLatestJournalId();
            _trace.TraceEvent(TraceEventType.Verbose, 0, "Latest journal ID: " + latest);
            var list =
                await
                    _eveApiService.GetNewJournalEntriesAsync(ApplicationHelper.ActiveKey,
                        ApplicationHelper.ActiveKeyEntity, latest);
            _trace.TraceEvent(TraceEventType.Verbose, 0, "Fetched journal entries: " + list.Count);
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Processing journal entries..."));
            await insertJournalEntries(list).ConfigureAwait(false);
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Journal update complete"));
            _trace.TraceEvent(TraceEventType.Stop, 0, "CompleteUpdateJournal");
        }

        private async Task insertJournalEntries(IEnumerable<JournalEntry> entries) {
            EveProfiteerDbEntities db;
            using (db = GetDb()) {
                db.Configuration.AutoDetectChangesEnabled = false;
                IList<JournalEntry> list = entries as IList<JournalEntry> ?? entries.ToList();
                int count = 0;
                foreach (JournalEntry entry in list) {
                    db.JournalEntries.Add(entry);
                    ++count;
                    if (count % 5000 != 0) continue;
                    db.ChangeTracker.DetectChanges();
                    await db.SaveChangesAsync().ConfigureAwait(false);
                    _trace.TraceEvent(TraceEventType.Verbose, 0, "Inserted journal entries: " + count);
                    db.Dispose();
                    db = GetDb();
                    db.Configuration.AutoDetectChangesEnabled = false;
                }
                db.ChangeTracker.DetectChanges();
                await db.SaveChangesAsync().ConfigureAwait(false);
            }
        }


        private async Task insertTransactions(IEnumerable<Transaction> transactions) {
            EveProfiteerDbEntities db;
            using (db = GetDb()) {
                db.Configuration.AutoDetectChangesEnabled = false;
                IList<Transaction> list = transactions as IList<Transaction> ?? transactions.ToList();
                int count = 0;
                foreach (Transaction transaction in list) {
                    db.Transactions.Add(transaction);
                    ++count;
                    if (count % 5000 != 0) continue;
                    db.ChangeTracker.DetectChanges();
                    await db.SaveChangesAsync().ConfigureAwait(false);
                    _trace.TraceEvent(TraceEventType.Verbose, 0, "Inserted transactions: " + count);
                    db.Dispose();
                    db = GetDb();
                    db.Configuration.AutoDetectChangesEnabled = false;
                }
                db.ChangeTracker.DetectChanges();
                await db.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private async Task updateInventory(IEnumerable<Transaction> transactions) {
            using (var db = GetDb()) {
                db.Configuration.AutoDetectChangesEnabled = false;
                List<Asset> assets =
                    await db.Assets.Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id).ToListAsync().ConfigureAwait(false);
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
                        asset.TotalCost += transaction.Price * transaction.Quantity;
                        asset.Quantity += transaction.Quantity;
                        asset.LatestAverageCost = asset.TotalCost / asset.Quantity;
                        transaction.PerpetualAverageCost = asset.LatestAverageCost;
                        transaction.CurrentStock = asset.Quantity;
                    } else if (transaction.TransactionType == TransactionType.Sell) {
                        if (asset.Quantity > 0) {
                            transaction.PerpetualAverageCost = asset.TotalCost / asset.Quantity;
                            asset.TotalCost -= transaction.Quantity * (asset.TotalCost / asset.Quantity);
                        } else {
                            transaction.PerpetualAverageCost = asset.LatestAverageCost;
                        }
                        asset.Quantity -= transaction.Quantity;
                        transaction.CurrentStock = asset.Quantity;
                        if (asset.Quantity < 0) {
                            transaction.UnaccountedStock += Math.Abs(asset.Quantity);
                            transaction.CurrentStock = 0;
                            asset.UnaccountedQuantity += Math.Abs(asset.Quantity);
                            asset.TotalCost = 0;
                            asset.Quantity = 0;
                        }
                    }
                }
                db.ChangeTracker.DetectChanges();
                await db.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}