using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using DevExpress.XtraPrinting.Native;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;
using OrderType = eZet.EveLib.Modules.OrderType;

namespace eZet.EveProfiteer.Services {
    public class OrderManagerService {
        private readonly EveMarketService _eveMarketService;
        private readonly EveProfiteerRepository _eveProfiteerRepository;

        private readonly TraceSource _trace = new TraceSource("EveLib", SourceLevels.All);

        public OrderManagerService(EveMarketService eveMarketService, EveProfiteerRepository eveProfiteerRepository) {
            _eveMarketService = eveMarketService;
            _eveProfiteerRepository = eveProfiteerRepository;
        }

        public Task<List<MapRegion>> GetRegions() {
            return _eveProfiteerRepository.GetRegionsOrdered().ToListAsync();
        }

        public Task<List<InvType>> GetMarketTypesAsync() {
            return _eveProfiteerRepository.GetMarketTypes().ToListAsync();
        }

        public async Task<List<OrderViewModel>> GetOrdersAsync() {
            var list = new List<OrderViewModel>();
            var orders = await _eveProfiteerRepository.MyOrders().Include(o => o.InvType.Assets.Select(a => a.LastSellTransaction)).Include(o => o.InvType.Assets.Select(a => a.LastBuyTransaction)).ToListAsync().ConfigureAwait(false);

            var marketOrders = await _eveProfiteerRepository.MyMarketOrders().Where(t => t.OrderState == OrderState.Open).ToListAsync().ConfigureAwait(false);
            var marketLookup = marketOrders.ToLookup(t => t.TypeId);

            foreach (var order in orders) {
                var ordervm = new OrderViewModel(order);
                order.InvType.Assets =
                    order.InvType.Assets.Where(f => f.ApiKeyEntity_Id == ApplicationHelper.ActiveEntity.Id)
                        .ToList();
                ordervm.HasActiveBuyOrder = marketLookup[order.TypeId].Any(t => t.Bid);
                ordervm.HasActiveSellOrder = marketLookup[order.TypeId].Any(t => !t.Bid);


                list.Add(ordervm);
            }
            return list;
        }

        public async Task<int> RemoveOrdersAsync(IEnumerable<OrderViewModel> orders) {
            var list = orders.Select(order => order.Order);
            var db = _eveProfiteerRepository.Context;
            foreach (var order in list) {
                if (order.Id == 0) {
                    continue;
                }
                db.Orders.Attach(order);
                db.Orders.Remove(order);
            }
            return await db.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<int> SaveOrdersAsync(IEnumerable<OrderViewModel> orders) {
            var db = _eveProfiteerRepository.Context;
            db.Configuration.AutoDetectChangesEnabled = false;
            db.Configuration.ValidateOnSaveEnabled = false;
            int count = 0;
            var list = orders.Select(f => f.Order).ToList();
            foreach (var order in list) {
                _trace.TraceEvent(TraceEventType.Verbose, 0, "" + count++);
                if (order.Id == 0) {
                    order.InvType = await db.InvTypes.FindAsync(order.InvType.TypeId).ConfigureAwait(false);
                    await db.Entry(order.InvType).Collection(f => f.Assets).LoadAsync().ConfigureAwait(false);
                    order.ApiKeyEntity_Id = ApplicationHelper.ActiveEntity.Id;
                    db.Orders.Add(order);
                } else {
                    //order.InvType = null;
                    db.Orders.Attach(order);
                    db.Entry(order).State = EntityState.Modified;
                }
            }
            db.ChangeTracker.DetectChanges();
            return await db.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task LoadMarketDataAsync(IEnumerable<OrderViewModel> orderViewModels, MapRegion region, StaStation station, int dayLimit) {
            var orders = orderViewModels.Select(f => f.Order);
            var enumerable = orders as IList<Order> ?? orders.ToList();
            var regionId = region != null ? region.RegionId : 0;
            var stationId = station != null ? station.StationId : 0;
            var pricesTask =
                _eveMarketService.GetItemPricesAsync(regionId, stationId,
                    enumerable.Select(o => o.TypeId)).ConfigureAwait(false);
            var historyTask = _eveMarketService.GetItemHistoryAsync(regionId,
                enumerable.Select(o => o.TypeId), dayLimit);
            var prices = await pricesTask;
            var priceLookup = prices.Prices.ToLookup(f => f.TypeId);
            var history = await historyTask;
            var historyLookup = history.ToLookup(f => f.TypeId);
            foreach (Order order in enumerable) {
                var itemHistory = historyLookup[order.TypeId].ToList();
                var price = priceLookup[order.TypeId].ToList();
                order.CurrentBuyPrice = price.Single(t => t.OrderType == OrderType.Buy).Price;
                order.CurrentSellPrice = price.Single(t => t.OrderType == OrderType.Sell).Price;
                if (!itemHistory.IsEmpty()) {
                    order.AvgPrice = itemHistory.Average(f => f.AvgPrice);
                    order.AvgVolume = itemHistory.Average(f => f.Volume);
                }
            }
        }
    }
}