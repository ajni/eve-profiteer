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

        public StationTradeAnalyzer GetStationTrader(Station station, ICollection<Item> items, int dayLimit) {
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
            var res = new StationTradeAnalyzer(items, sellOrders, buyOrders, history);
            res.Analyze();
            return res;
        }

        public async Task<MarketAnalyzer> GetMarketAnalyzer(Region region, ICollection<Item> items, int days, IProgress<ProgressType> progress) {
            progress.Report(new ProgressType(0, "Configuring query..."));
            var options = new EveMarketDataOptions();
            options.Regions.Add(region.RegionId);
            options.AgeSpan = TimeSpan.FromDays(days);
            var result = new MarketAnalyzer(region, items);
            var tasks = new List<Task<EveMarketDataResponse<ItemHistory>>>();
            progress.Report(new ProgressType(25, "Fetching history data..."));
            foreach (var item in items) {
                options.Items.Add(item.TypeId);
                if (options.Items.Count > 1000) {
                    tasks.Add(getItemHistory(options));
                    options.Items.Clear();
                }
            }
            progress.Report(new ProgressType(50, "Initializing analysis..."));
            foreach (var res in await Task.WhenAll(tasks)) {
                result.Add(res.Result.History);
            }
            progress.Report(new ProgressType(75, "Analyzing..."));
            result.Calculate();
            progress.Report(new ProgressType(100, "Finished."));
            return result;
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
