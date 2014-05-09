using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using eZet.Eve.EveProfiteer.Entities;
using eZet.EveLib.Modules;
using eZet.EveLib.Modules.Models;
using eZet.EveProfiteer.Models;
using OrderType = eZet.EveLib.Modules.OrderType;

namespace eZet.EveProfiteer.Services {

    public class EveMarketService {

        private EveMarketData eveMarketData { get; set; }

        private EveCentral eveCentral { get; set; }
        private int Jita { get; set; }

        public EveMarketService() {
            eveMarketData = new EveMarketData(Format.Json);
            eveCentral = new EveCentral();
            Jita = 60003760;
        }

    

        public MarketAnalyzer GetStationTrader(Station station, ICollection<Item> items, int dayLimit) {
            var historyOptions = new EveMarketDataOptions();
            historyOptions.AgeSpan = TimeSpan.FromDays(dayLimit);
            historyOptions.Regions.Add(station.RegionId);
            var priceOptions = new EveMarketDataOptions();
            priceOptions.Stations.Add(station.StationId);
            var sellOrders = new List<ItemPrices.ItemPriceEntry>();
            var buyOrders = new List<ItemPrices.ItemPriceEntry>();
            var history = new List<ItemHistory.ItemHistoryEntry>();
            var tasks = new List<Task>();
            foreach (var item in items) {
                historyOptions.Items.Add(item.TypeId);
                priceOptions.Items.Add(item.TypeId);
                if (historyOptions.Items.Count > 1000) {
                    history.AddRange(eveMarketData.GetItemHistory(historyOptions).Result.History);
                    historyOptions.Items.Clear();
                }
                if (priceOptions.Items.Count > 1000) {
                    sellOrders.AddRange(eveMarketData.GetItemPrice(priceOptions, OrderType.Sell).Result.Prices);
                    buyOrders.AddRange(eveMarketData.GetItemPrice(priceOptions, OrderType.Buy).Result.Prices);
                    priceOptions.Items.Clear();
                }

            }
            sellOrders.AddRange(eveMarketData.GetItemPrice(priceOptions, OrderType.Sell).Result.Prices); ;
            buyOrders.AddRange(eveMarketData.GetItemPrice(priceOptions, OrderType.Buy).Result.Prices);
            history.AddRange(eveMarketData.GetItemHistory(historyOptions).Result.History);
            var res = new MarketAnalyzer(items, sellOrders, buyOrders, history);
            res.Analyze();
            return res;
        }

        public Uri GetScannerLink(ICollection<Int64> items) {
            var options = new EveMarketDataOptions { Items = items };
            return eveMarketData.GetScannerUri(options);
        }

        private async Task<EveMarketDataResponse<ItemHistory>> getItemHistory(EveMarketDataOptions options) {
            return await Task.Run(() => eveMarketData.GetItemHistory(options));
        }
    }
}
