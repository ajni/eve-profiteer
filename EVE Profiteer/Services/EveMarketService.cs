using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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


        public MarketBrowserItem GetDetails(MapRegion region, InvType invType) {
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
                }
                else {
                    sellOrders.Add(marketOrder);
                }
            }
            MarketHistoryResponse marketHistoryResponse = EveCrest.GetMarketHistory(region.RegionId, invType.TypeId);
            var marketHistory = new List<MarketHistoryEntry>();
            foreach (MarketHistoryResponse.MarketHistoryEntry entry in marketHistoryResponse.Entries) {
                marketHistory.Add(ApiEntityMapper.Map(entry, new MarketHistoryEntry()));
            }


            var item = new MarketBrowserItem(invType, marketHistory, sellOrders, buyOrders, 20);
            return item;
        }

        public MarketAnalyzer GetMarketAnalyzer(MapRegion region, StaStation station, ICollection<InvType> items,
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
            var tasks = new List<Task>();
            foreach (InvType item in items) {
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
            sellOrders.AddRange(eveMarketData.GetItemPrice(priceOptions, OrderType.Sell).Result.Prices);
            ;
            buyOrders.AddRange(eveMarketData.GetItemPrice(priceOptions, OrderType.Buy).Result.Prices);
            history.AddRange(eveMarketData.GetItemHistory(historyOptions).Result.History);
            var res = new MarketAnalyzer(items, sellOrders, buyOrders, history);
            res.Analyze();
            return res;
        }

        public Uri GetScannerLink(ICollection<Int64> items) {
            var options = new EveMarketDataOptions {Items = items};
            return eveMarketData.GetScannerUri(options);
        }

        public void LoadMarketData(ICollection<Order> orders, int dayLimit, int region = 10000002) {
            if (orders.Count == 0) return;
            var historyOptions = new EveMarketDataOptions();
            foreach (Order order in orders) {
                historyOptions.Items.Add(order.TypeId);
            }
            historyOptions.AgeSpan = TimeSpan.FromDays(dayLimit);
            historyOptions.Regions.Add(region);
            EveMarketDataResponse<ItemHistory> history = eveMarketData.GetItemHistory(historyOptions);
            ILookup<long, ItemHistory.ItemHistoryEntry> historyLookup = history.Result.History.ToLookup(f => f.TypeId);
            foreach (Order order in orders) {
                List<ItemHistory.ItemHistoryEntry> itemHistory = historyLookup[order.TypeId].ToList();
                if (!itemHistory.IsEmpty()) {
                    order.AvgPrice = itemHistory.Average(f => f.AvgPrice);
                    order.AvgVolume = itemHistory.Average(f => f.Volume);
                }
            }
        }
    }
}