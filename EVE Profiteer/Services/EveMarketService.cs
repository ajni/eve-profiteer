using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Caliburn.Micro;
using eZet.EveLib.Modules;
using eZet.EveLib.Modules.Models;
using eZet.EveProfiteer.Models;
using OrderType = eZet.EveLib.Modules.OrderType;

namespace eZet.EveProfiteer.Services {
    public class EveMarketService {
        private readonly EveCrest _eveCrest = new EveCrest();
        private readonly EveMarketData _eveMarketData = new EveMarketData();


        public async Task<EveMarketDataResponse<ItemOrders>> GetItemOrderAsync(int region, int invType) {
            var options = new EveMarketDataOptions();
            options.Items.Add(invType);
            options.Regions.Add(region);
            return await _eveMarketData.GetItemOrdersAsync(options, OrderType.Both).ConfigureAwait(false);
        }

        public async Task<ItemOrders> GetItemOrdersAsync(int region, IEnumerable<int> invTypes) {
            var options = new EveMarketDataOptions();
            var orders = new ItemOrders();
            options.Regions.Add(region);
            foreach (var item in invTypes) {
                options.Items.Add(item);
                if (options.Items.Count >= 1000) {
                    var result = (await _eveMarketData.GetItemOrdersAsync(options, OrderType.Both).ConfigureAwait(false)).Result;
                    result.Orders.Apply(order => orders.Orders.Add(order));
                    options.Items.Clear();
                }
            }
            var finalResult =  (await _eveMarketData.GetItemOrdersAsync(options, OrderType.Both).ConfigureAwait(false)).Result;
            finalResult.Orders.Apply(order => orders.Orders.Add(order));
            return orders;
        }

        public async Task<MarketHistoryResponse> GetMarketHistoryAsync(int region, int invType) {
            return await _eveCrest.GetMarketHistoryAsync(region, invType).ConfigureAwait(false);
        }


        public async Task<MarketAnalyzer> GetMarketAnalyzerData(MapRegion region, StaStation station,
            ICollection<InvType> items,
            int dayLimit) {
            var historyOptions = new EveMarketDataOptions();
            historyOptions.AgeSpan = TimeSpan.FromDays(dayLimit);
            historyOptions.Regions.Add(region.RegionId);
            var priceOptions = new EveMarketDataOptions();
            if (station != null)
                priceOptions.Stations.Add(station.StationId);
            else
                priceOptions.Regions.Add(region.RegionId);
            var sellOrders = new List<ItemPrices.ItemPriceEntry>();
            var buyOrders = new List<ItemPrices.ItemPriceEntry>();
            var history = new List<ItemHistory.ItemHistoryEntry>();
            foreach (InvType item in items) {
                historyOptions.Items.Add(item.TypeId);
                priceOptions.Items.Add(item.TypeId);
                if (historyOptions.Items.Count > 1000) {
                    EveMarketDataResponse<ItemHistory> response =
                        await _eveMarketData.GetItemHistoryAsync(historyOptions).ConfigureAwait(false);
                    history.AddRange(response.Result.History);
                    historyOptions.Items.Clear();
                }
                if (priceOptions.Items.Count > 1000) {
                    EveMarketDataResponse<ItemPrices> sellOrdersTask =
                        await _eveMarketData.GetItemPriceAsync(priceOptions, OrderType.Sell).ConfigureAwait(false);
                    sellOrders.AddRange(sellOrdersTask.Result.Prices);
                    EveMarketDataResponse<ItemPrices> buyOrdersTask =
                        await _eveMarketData.GetItemPriceAsync(priceOptions, OrderType.Buy).ConfigureAwait(false);
                    buyOrders.AddRange(buyOrdersTask.Result.Prices);
                    priceOptions.Items.Clear();
                }
            }
            sellOrders.AddRange(_eveMarketData.GetItemPrice(priceOptions, OrderType.Sell).Result.Prices);
            buyOrders.AddRange(_eveMarketData.GetItemPrice(priceOptions, OrderType.Buy).Result.Prices);
            history.AddRange(_eveMarketData.GetItemHistory(historyOptions).Result.History);
            var res = new MarketAnalyzer(items, sellOrders, buyOrders, history);
            return res;
        }

        public Uri GetScannerLink(ICollection<int> items) {
            var options = new EveMarketDataOptions {Items = items};
            return _eveMarketData.GetScannerUri(options);
        }

        public async Task<ItemPrices> GetItemPricesAsync(int stationId, IEnumerable<int> types) {
            var prices = new ItemPrices();
            prices.Prices = new EveMarketDataRowCollection<ItemPrices.ItemPriceEntry>();
            var options = new EveMarketDataOptions();
            options.Stations.Add(stationId);
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

        public async Task<ItemHistory> GetItemHistoryAsync(int region, IEnumerable<int> types, int dayLimit) {
            var history = new ItemHistory();
            history.History = new EveMarketDataRowCollection<ItemHistory.ItemHistoryEntry>();
            var options = new EveMarketDataOptions();
            options.AgeSpan = TimeSpan.FromDays(dayLimit);
            options.Regions.Add(region);
            foreach (int typeId in types) {
                options.Items.Add(typeId);
                if (options.Items.Count >= 1000) {
                    var result = (await _eveMarketData.GetItemHistoryAsync(options).ConfigureAwait(false)).Result;
                    result.History.Apply(item => history.History.Add(item));
                    options.Items.Clear();
                }
            }
            var finalResult = (await _eveMarketData.GetItemHistoryAsync(options).ConfigureAwait(false)).Result;
            finalResult.History.Apply(item => history.History.Add(item));
            return history;
        }
    }
}