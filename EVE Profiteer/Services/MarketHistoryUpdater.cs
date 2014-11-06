using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eve_Static_data;
using eZet.EveData.MarketData;
using eZet.EveLib.Modules;
using eZet.EveLib.Modules.Models;

namespace eZet.EveProfiteer.Services {
    public class MarketHistoryUpdater {

        private BlockingCollection<ConsumerTask> Queue { get; set; }

        private EveCrest EveCrest { get; set; }
        private EveStaticData StaticDataContext { get; set; }


        public MarketHistoryUpdater() {
            EveCrest = new EveCrest();
            StaticDataContext = new EveStaticData();
            Queue = new BlockingCollection<ConsumerTask>();
        }

        private Task consume() {
            Console.WriteLine("MarketHistoryUpdater: Starting...");
            int count = 0;
            var tasks = new List<Task>();
            while (!Queue.IsCompleted) {
                ConsumerTask task;
                try {
                    task = Queue.Take();
                } catch (Exception) {
                    break;
                }
                tasks.Add(insert(task));
                ++count;
                if (tasks.Count >= 10) {
                    Task.WaitAll(tasks.ToArray());
                    tasks.Clear();
                    if (count % 100 == 0) {
                        Console.WriteLine("Inserted: " + count);
                        Console.WriteLine("Queue: " + Queue.Count);
                    }
                }
            }
            Console.WriteLine("MarketHistoryUpdater: Finished: " + count);
            return Task.FromResult(true);
        }

        private static async Task insert(ConsumerTask task) {
            foreach (var historyEntry in task.Entries) {
                using (var marketDataContext = new EveMarketDataContext()) {
                    marketDataContext.Configuration.AutoDetectChangesEnabled = false;
                    marketDataContext.Configuration.ValidateOnSaveEnabled = false;
                    marketDataContext.MarketHistoryEntries.Add(map(task.TypeId, task.RegionId, historyEntry));
                    await marketDataContext.SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        private static EveData.MarketData.MarketHistoryEntry map(int typeId, int regionId, CrestMarketHistory.MarketHistoryEntry source) {
            var target = new EveData.MarketData.MarketHistoryEntry();
            target.TypeId = typeId;
            target.RegionId = regionId;
            target.AvgPrice = source.AvgPrice;
            target.HighPrice = source.HighPrice;
            target.LowPrice = source.LowPrice;
            target.Volume = source.Volume;
            target.OrderCount = source.OrderCount;
            target.Date = source.Date;
            return target;
        }

        private async Task produce(int region, int typeId, EveData.MarketData.MarketHistoryEntry latest) {
            CrestMarketHistory result;
            while (true) {
                try {
                    result = await EveCrest.GetMarketHistoryAsync(region, typeId).ConfigureAwait(false);
                    break;
                } catch (Exception) {
                    Console.WriteLine("Something went wrong, waiting...");
                    Thread.Sleep(30000);
                }
            }
            var limit = DateTime.UtcNow.AddMonths(-3);
            var history = result.Entries.Where(e => e.Date > limit).ToList();
            if (latest != null) {
                history = history.Where(e => e.Date > latest.Date).ToList();
            }
            Queue.Add(new ConsumerTask(typeId, region, history));
        }

        public async Task update(IEnumerable<int> types, IEnumerable<int> regions) {
            Task consumer = Task.Factory.StartNew(() => consume());
            //var types =
            //    StaticDataContext.invTypes.Where(
            //        e => e.typeID < 350000 && e.published == true && e.marketGroupID != null).AsNoTracking();
            Console.WriteLine("Fetching: " + types.Count());
            DateTime timer = DateTime.UtcNow;
            var tasks = new List<Task>();
            int processed = 0;
            foreach (var region in regions) {
                Console.WriteLine("RegionId: " + region);
                foreach (var typeId in types) {
                    DateTime now = DateTime.UtcNow;
                    //var typeId = type.typeID;
                    EveData.MarketData.MarketHistoryEntry latest;
                    using (var marketDataContext = new EveMarketDataContext()) {
                        latest = marketDataContext.MarketHistoryEntries.AsNoTracking()
                            .Where(e => e.TypeId == typeId)
                            .OrderByDescending(e => e.Date)
                            .FirstOrDefault();
                    }

                    if (latest != null && latest.Date.Date >= now.Date.AddDays(-1)) {
                        continue;
                    }

                    tasks.Add(produce(region, typeId, latest));
                    ++processed;
                    if (processed % 100 == 0) Console.WriteLine("Processed: " + processed);

                    if (tasks.Count == 10) {
                        Task.WaitAll(tasks.ToArray());
                        tasks.Clear();
                        double timeSpent = (DateTime.UtcNow - timer).TotalMilliseconds;
                        var sleepTimer = (int)(1000 - timeSpent);
                        if (sleepTimer > 0) {
                            Thread.Sleep(sleepTimer);
                        }
                        timer = DateTime.UtcNow;
                    }
                }
            }
            Task.WaitAll(tasks.ToArray());
            Queue.CompleteAdding();
            await consumer;
        }


        private class ConsumerTask {

            public ConsumerTask(int typeId, int regionId, IList<CrestMarketHistory.MarketHistoryEntry> entries) {
                TypeId = typeId;
                RegionId = regionId;
                Entries = entries;
            }

            public int RegionId { get; private set; }

            public int TypeId { get; private set; }

            public IList<CrestMarketHistory.MarketHistoryEntry> Entries { get; private set; }
        }

    }

}
