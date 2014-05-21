using System.Collections.Generic;
using eZet.EveLib.Modules;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;

namespace eZet.EveProfiteer.Services {
    public class MarketBrowserService {

        public MarketBrowserService() {
            EveCrest = new EveCrest();
            EveMarketData = new EveMarketData(name: "EveProfiteer");
        }

        public EveCrest EveCrest { get; private set; }

        public EveMarketData EveMarketData { get; private set; }

        public MarketBrowserItem GetDetails(mapRegion region, InvType invType) {
            var options = new EveMarketDataOptions();
            options.Items.Add(invType.TypeId);
            options.Regions.Add(region.RegionId);
            var orderResponse = EveMarketData.GetItemOrders(options, OrderType.Both);
            var buyOrders = new List<MarketOrder>();
            var sellOrders = new List<MarketOrder>();
            foreach (var order in orderResponse.Result.Orders) {
                var marketOrder = ApiEntityMapper.Map(order, new MarketOrder());
                if (order.OrderType == OrderType.Buy) {
                    buyOrders.Add(marketOrder);
                } else {
                    sellOrders.Add(marketOrder);
                }
            }
            var marketHistoryResponse = EveCrest.GetMarketHistory(region.RegionId, invType.TypeId);
            var marketHistory = new List<MarketHistoryEntry>();
            foreach (var entry in marketHistoryResponse.Entries) {
                marketHistory.Add(ApiEntityMapper.Map(entry, new MarketHistoryEntry()));
            }


            var item = new MarketBrowserItem(invType, marketHistory, sellOrders, buyOrders, 20);
            return item;
        }


    }
}
