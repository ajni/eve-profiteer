using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.XtraPrinting.Native;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Repository;
using eZet.EveProfiteer.Util;
using OrderType = eZet.EveLib.Modules.OrderType;

namespace eZet.EveProfiteer.Services {
    public class OrderManagerService : DbContextService {
        private readonly EveMarketService _eveMarketService;
        private readonly EveStaticDataRepository _staticData;

        private readonly TraceSource _trace = new TraceSource("EveLib", SourceLevels.All);

        public OrderManagerService(EveMarketService eveMarketService, EveStaticDataRepository staticData) {
            _eveMarketService = eveMarketService;
            _staticData = staticData;
        }

        public Task<List<MapRegion>> GetRegions() {
            return _staticData.GetRegionsOrdered().ToListAsync();
        }

        public Task<List<InvType>> GetMarketTypesAsync() {
            return Db.GetMarketTypes().ToListAsync();
        }

        public async Task<List<OrderVm>> GetOrdersAsync(int stationId) {
            var list = new List<OrderVm>();
            var orders = await Db.MyOrders().Where(o => o.StationId == stationId).Include(o => o.InvType.Assets).ToListAsync().ConfigureAwait(false);
            var marketOrders = await Db.MyMarketOrders().Where(t => t.OrderState == OrderState.Open).ToListAsync().ConfigureAwait(false);
            var marketLookup = marketOrders.ToLookup(t => t.TypeId);

            foreach (var order in orders) {
                order.InvType.Assets =
                    order.InvType.Assets.Where(f => f.ApiKeyEntity_Id == ApplicationHelper.ActiveEntity.Id)
                        .ToList();
                var sellOrder = marketLookup[order.TypeId].SingleOrDefault(t => !t.Bid);
                var buyOrder = marketLookup[order.TypeId].SingleOrDefault(t => t.Bid);
                var ordervm = new OrderVm(stationId, order, buyOrder, sellOrder);
                list.Add(ordervm);
            }
            return list;
        }

        public async Task<int> RemoveOrdersAsync(IEnumerable<OrderVm> orders) {
            var list = orders.Select(order => order.Order);
            var db = Db.Context;
            foreach (var order in list) {
                if (order.Id == 0) {
                    continue;
                }
                db.Orders.Attach(order);
                db.Orders.Remove(order);
            }
            return await db.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<int> SaveOrdersAsync(IEnumerable<OrderVm> orders) {
            var db = Db.Context;
            db.Configuration.AutoDetectChangesEnabled = false;
            db.Configuration.ValidateOnSaveEnabled = false;
            int count = 0;
            var list = orders.Select(f => f.Order).ToList();
            foreach (var order in list) {
                _trace.TraceEvent(TraceEventType.Verbose, 0, "" + count++);
                if (order.Id == 0) {
                    if (order.InvType == null) {
                        order.InvType = await db.InvTypes.FindAsync(order.TypeId).ConfigureAwait(false);
                        await db.Entry(order.InvType).Collection(f => f.Assets).LoadAsync().ConfigureAwait(false);
                    }
                    order.ApiKeyEntity_Id = ApplicationHelper.ActiveEntity.Id;
                    db.Orders.Add(order);
                } else {
                    //order.InvType = null;
                    //db.Orders.Attach(order);
                    db.Entry(order).State = EntityState.Modified;
                }
            }
            return await db.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task LoadInvTypes(IEnumerable<Order> orders) {
            IList<Order> enumerable = orders as IList<Order> ?? orders.ToList();
            IEnumerable<int> orderIds = enumerable.Select(f => f.TypeId);
            var types = await Db.Context.InvTypes.Where(f => orderIds.Contains(f.TypeId)).ToListAsync().ConfigureAwait(false);
            ILookup<int, InvType> lookup = types.ToLookup(f => f.TypeId);
            foreach (Order order in enumerable) {
                order.InvType = lookup[order.TypeId].Single();
            }
        }

        public async Task LoadMarketDataAsync(IEnumerable<OrderVm> orderViewModels, MapRegion region, StaStation station, int dayLimit) {
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