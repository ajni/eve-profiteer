using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using eZet.EveData.MarketData;
using eZet.EveLib.Modules;
using eZet.EveLib.Modules.Models;
using MarketHistoryEntry = eZet.EveProfiteer.Models.MarketHistoryEntry;
using OrderType = eZet.EveLib.Modules.OrderType;

namespace eZet.EveProfiteer.Services {
    public class EveMarketService {
        private readonly EveCrest _eveCrest = new EveCrest();
        private readonly EveMarketData _eveMarketData = new EveMarketData();


        public async Task<EveMarketDataResponse<EmdItemOrders>> GetItemOrderAsync(int region, int invType) {
            var options = new EveMarketDataOptions();
            options.Items.Add(invType);
            options.Regions.Add(region);
            return await _eveMarketData.GetItemOrdersAsync(options, OrderType.Both).ConfigureAwait(false);
        }

        public async Task<EmdItemOrders> GetItemOrdersAsync(int region, IEnumerable<int> invTypes) {
            var options = new EveMarketDataOptions();
            var orders = new EmdItemOrders();
            options.Regions.Add(region);
            foreach (var item in invTypes) {
                options.Items.Add(item);
                if (options.Items.Count >= 1000) {
                    var result = (await _eveMarketData.GetItemOrdersAsync(options, OrderType.Both).ConfigureAwait(false)).Result;
                    result.Orders.Apply(order => orders.Orders.Add(order));
                    options.Items.Clear();
                }
            }
            var finalResult = (await _eveMarketData.GetItemOrdersAsync(options, OrderType.Both).ConfigureAwait(false)).Result;
            finalResult.Orders.Apply(order => orders.Orders.Add(order));
            return orders;
        }

        public async Task<CrestMarketHistory> GetMarketHistoryAsync(int region, int invType) {
            return await _eveCrest.GetMarketHistoryAsync(region, invType).ConfigureAwait(false);
        }

        public async Task<EmdItemPrices> GetItemPricesAsync(int regionId, int stationId, IEnumerable<int> types) {
            var prices = new EmdItemPrices();
            prices.Prices = new EveMarketDataRowCollection<EmdItemPrices.ItemPriceEntry>();
            var options = new EveMarketDataOptions();
            if (stationId > 0)
                options.Stations.Add(stationId);
            else if (regionId > 0)
                options.Regions.Add(regionId);
            foreach (int typeId in types) {
                options.Items.Add(typeId);
                if (options.Items.Count >= 1000) {
                    var result = (await _eveMarketData.GetItemPriceAsync(options, OrderType.Both).ConfigureAwait(false)).Result;
                    result.Prices.Apply(item => prices.Prices.Add(item));
                    options.Items.Clear();
                }
            }
            var finalResult = (await _eveMarketData.GetItemPriceAsync(options, OrderType.Both).ConfigureAwait(false)).Result;
            finalResult.Prices.Apply(item => prices.Prices.Add(item));
            return prices;
        }

        public async Task<IEnumerable<MarketHistoryEntry>> GetItemHistoryAsync(int region, IEnumerable<int> types,
            int dayLimit) {
            var source = Properties.Settings.Default.MarketHistorySource;
            if (source == "Crest") return await getCrestItemHistoryAsync(region, types, dayLimit);
            else return await getEmdItemHistoryAsync(region, types, dayLimit);
        }

        private async Task<IEnumerable<MarketHistoryEntry>> getCrestItemHistoryAsync(int region, IEnumerable<int> types, int dayLimit) {
            var updater = new MarketHistoryUpdater();
            await updater.update(types, new[] { region });
            List<EveData.MarketData.MarketHistoryEntry> list;
            using (var marketDataContext = new EveMarketDataContext()) {
                var limit = DateTime.UtcNow.AddDays(-dayLimit);
                list = await marketDataContext.MarketHistoryEntries.AsNoTracking()
                   .Where(e => types.Contains(e.TypeId) && e.Date > limit)
                   .ToListAsync()
                   .ConfigureAwait(false);
            }
            return list.Select(MarketHistoryEntry.Create);
        }

        private async Task<IEnumerable<MarketHistoryEntry>> getEmdItemHistoryAsync(int region, IEnumerable<int> types, int dayLimit) {
            var history = new List<EmdItemHistory.ItemHistoryEntry>();
            var options = new EveMarketDataOptions();
            options.AgeSpan = TimeSpan.FromDays(dayLimit);
            options.Regions.Add(region);
            foreach (int typeId in types) {
                options.Items.Add(typeId);
                if (options.Items.Count * dayLimit >= 9900) {
                    var result = (await _eveMarketData.GetItemHistoryAsync(options).ConfigureAwait(false)).Result;
                    history.AddRange(result.History);
                    options.Items.Clear();
                }
            }
            var finalResult = (await _eveMarketData.GetItemHistoryAsync(options).ConfigureAwait(false)).Result;
            history.AddRange(finalResult.History);
            return history.Select(MarketHistoryEntry.Create);
        }
    }
}