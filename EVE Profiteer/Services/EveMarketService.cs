using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Caliburn.Micro;
using eZet.EveLib.Modules;
using eZet.EveLib.Modules.Models;
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