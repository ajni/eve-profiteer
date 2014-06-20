﻿using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.XtraPrinting.Native;
using eZet.EveLib.Modules.Models;
using eZet.EveProfiteer.Models;
using OrderType = eZet.EveLib.Modules.OrderType;

namespace eZet.EveProfiteer.Services {
    public class OrderEditorService : DbContextService {
        private readonly EveMarketService _eveMarketService;

        public OrderEditorService(EveMarketService eveMarketService) {
            _eveMarketService = eveMarketService;
        }

        public async Task<List<InvType>> GetMarketTypes() {
            using (var db = CreateDb()) {
                return await GetMarketTypes(db).AsNoTracking().OrderBy(t => t.TypeName).ToListAsync().ConfigureAwait(false);
            }
        }

        public async Task<List<Order>> GetOrders() {
            using (var db = CreateDb()) {
                return await MyOrders(db).Include("InvType").Include("InvType.Assets").ToListAsync().ConfigureAwait(false);
            }
        }

        public async Task LoadMarketDataAsync(IEnumerable<Order> orders, int dayLimit) {
            var enumerable = orders as IList<Order> ?? orders.ToList();
            var pricesTask = _eveMarketService.GetItemPricesAsync(Properties.Settings.Default.DefaultStationId, enumerable.Select(o => o.TypeId)).ConfigureAwait(false);
            var historyTask = _eveMarketService.GetItemHistoryAsync(Properties.Settings.Default.DefaultRegionId, enumerable.Select(o => o.TypeId), dayLimit);
            var prices = await pricesTask;
            var priceLookup = prices.Prices.ToLookup(f => f.TypeId);
            var history = await historyTask;
            ILookup<int, EmdItemHistory.ItemHistoryEntry> historyLookup = history.History.ToLookup(f => f.TypeId);
            foreach (Order order in enumerable) {
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
    }
}