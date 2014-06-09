using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using DevExpress.XtraPrinting.Native;
using eZet.EveLib.Modules;
using eZet.EveLib.Modules.Models;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;
using OrderType = eZet.EveLib.Modules.OrderType;

namespace eZet.EveProfiteer.Services {
    public class EveMarketService {
        public EveMarketService() {
            eveMarketData = new EveMarketData();
            eveCentral = new EveCentral();
            EveCrest = new EveCrest();
        }

        private EveMarketData eveMarketData { get; set; }

        private EveCentral eveCentral { get; set; }

        public EveCrest EveCrest { get; private set; }


        public MarketBrowserItem GetDetails(InvType invType, MapRegion region) {
            var options = new EveMarketDataOptions();
            options.Items.Add(invType.TypeId);
            options.Regions.Add(region.RegionId);
            EveMarketDataResponse<ItemOrders> orderResponse = eveMarketData.GetItemOrders(options, OrderType.Both);
            var buyOrders = new List<MarketOrder>();
            var sellOrders = new List<MarketOrder>();
            foreach (ItemOrders.ItemOrderEntry order in orderResponse.Result.Orders) {
                MarketOrder marketOrder = ApiEntityMapper.Map(order, new MarketOrder());
                if (order.OrderType == OrderType.Buy) {
                    buyOrders.Add(marketOrder);
                } else {
                    sellOrders.Add(marketOrder);
                }
            }
            MarketHistoryResponse marketHistoryResponse = EveCrest.GetMarketHistory(region.RegionId, invType.TypeId);
            var marketHistory = new List<MarketHistoryEntry>();
            foreach (MarketHistoryResponse.MarketHistoryEntry entry in marketHistoryResponse.Entries) {
                marketHistory.Add(ApiEntityMapper.Map(entry, new MarketHistoryEntry()));
            }


            var item = new MarketBrowserItem(invType, marketHistory, sellOrders, buyOrders, 7);
            return item;
        }

        public async Task<MarketAnalyzer> GetMarketAnalyzerData(MapRegion region, StaStation station, ICollection<InvType> items,
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
                    var response = await eveMarketData.GetItemHistoryAsync(historyOptions).ConfigureAwait(false);
                    history.AddRange(response.Result.History);
                    historyOptions.Items.Clear();
                }
                if (priceOptions.Items.Count > 1000) {
                    var sellOrdersTask =
                        await eveMarketData.GetItemPriceAsync(priceOptions, OrderType.Sell).ConfigureAwait(false);
                    sellOrders.AddRange(sellOrdersTask.Result.Prices);
                    var buyOrdersTask =
                        await eveMarketData.GetItemPriceAsync(priceOptions, OrderType.Buy).ConfigureAwait(false);
                    buyOrders.AddRange(buyOrdersTask.Result.Prices);
                    priceOptions.Items.Clear();
                }
            }
            sellOrders.AddRange(eveMarketData.GetItemPrice(priceOptions, OrderType.Sell).Result.Prices);
            buyOrders.AddRange(eveMarketData.GetItemPrice(priceOptions, OrderType.Buy).Result.Prices);
            history.AddRange(eveMarketData.GetItemHistory(historyOptions).Result.History);
            var res = new MarketAnalyzer(items, sellOrders, buyOrders, history);
            return res;
        }

        public Uri GetScannerLink(ICollection<int> items) {
            var options = new EveMarketDataOptions { Items = items };
            return eveMarketData.GetScannerUri(options);
        }

        public Task LoadMarketDataAsync(IEnumerable<Order> enumerable, int dayLimit, int region = 10000002) {
            return Task.Run(() => loadMarketData(enumerable, dayLimit, region));
        }

        private async Task loadMarketData(IEnumerable<Order> enumerable, int dayLimit, int region = 10000002) {
            var orders = enumerable as IList<Order> ?? enumerable.ToList();
            if (orders.Count == 0) return;
            var options = new EveMarketDataOptions();
            foreach (Order order in orders) {
                options.Items.Add(order.TypeId);
            }
            options.AgeSpan = TimeSpan.FromDays(dayLimit);
            options.AgeSpan = TimeSpan.FromDays(10);
            options.Stations.Add(Properties.Settings.Default.DefaultStationId);
            var pricesTask = eveMarketData.GetItemPriceAsync(options, OrderType.Both);
            options.Regions.Add(region);
            var historyTask = eveMarketData.GetItemHistoryAsync(options);
            var prices = await pricesTask;
            var priceLookup = prices.Result.Prices.ToLookup(f => f.TypeId);
            var history = await historyTask;
            ILookup<int, ItemHistory.ItemHistoryEntry> historyLookup = history.Result.History.ToLookup(f => f.TypeId);
            foreach (Order order in orders) {
                var itemHistory = historyLookup[order.TypeId].ToList();
                var price = priceLookup[order.TypeId];
                if (!itemHistory.IsEmpty()) {
                    order.AvgPrice = itemHistory.Average(f => f.AvgPrice);
                    order.AvgVolume = itemHistory.Average(f => f.Volume);
                    order.CurrentBuyPrice = price.Single(t => t.OrderType == OrderType.Buy).Price;
                    order.CurrentSellPrice = price.Single(t => t.OrderType == OrderType.Sell).Price;
                }
            }
        }

        public async Task<EveMarketDataRowCollection<ItemPrices.ItemPriceEntry>> GetPriceData(IEnumerable<int> types,
            int station) {
            var options = new EveMarketDataOptions();
            types.Apply(type => options.Items.Add(type));
            options.Stations.Add(station);
            var res = await eveMarketData.GetItemPriceAsync(options, OrderType.Both).ConfigureAwait(false);
            return res.Result.Prices;
        }

        public async Task<EveMarketDataRowCollection<ItemHistory.ItemHistoryEntry>> GetHistoryData(IEnumerable<int> types,
    int region, int days) {
            var options = new EveMarketDataOptions();
            types.Apply(type => options.Items.Add(type));
            options.Regions.Add(region);
            options.AgeSpan = TimeSpan.FromDays(days);
            var res = await eveMarketData.GetItemHistoryAsync(options).ConfigureAwait(false);
            return res.Result.History;
        }

    }
}