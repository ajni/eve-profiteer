using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DevExpress.XtraPrinting.Native;
using eZet.Eve.OrderIoHelper;
using eZet.Eve.OrderIoHelper.Models;
using eZet.EveLib.Modules;
using eZet.EveLib.Modules.Models;
using eZet.EveOnlineDbModels;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Repository;

namespace eZet.EveProfiteer.Services {
    public class OrderEditorService {
        private readonly EveDbContext _eveDbContext;
        private readonly IRepository<OrderData> _ordersRepository;
        public string BuyOrdersFileName = "BuyOrders.xml";

        public string SellOrdersFileName = "SellOrders.xml";


        public OrderEditorService(EveDbContext eveDbContext, IRepository<OrderData> ordersRepository) {
            _eveDbContext = eveDbContext;
            _ordersRepository = ordersRepository;
            EveMarketData = new EveMarketData();
        }

        private EveMarketData EveMarketData { get; set; }

        public ICollection<OrderData> LoadOrdersFromDisk(string path) {
            var orders = new List<OrderData>();
            try {
                BuyOrderCollection buyOrders =
                    OrderInstallerIoService.ReadBuyOrders(path + Path.DirectorySeparatorChar + BuyOrdersFileName);
                SellOrderCollection sellOrders =
                    OrderInstallerIoService.ReadSellOrders(path + Path.DirectorySeparatorChar + SellOrdersFileName);
                ILookup<int, SellOrder> sellOrderLookup = sellOrders.ToLookup(f => f.TypeId);
                foreach (BuyOrder buyOrder in buyOrders) {
                    SellOrder sellOrder = sellOrderLookup[buyOrder.ItemId].SingleOrDefault();
                    sellOrders.Remove(sellOrder);
                    orders.Add(CreateOrder(buyOrder, sellOrder));
                }
                foreach (SellOrder sellOrder in sellOrders) {
                    orders.Add(CreateOrder(null, sellOrder));
                }
            }
            catch (FileNotFoundException) {
            }
            loadType(orders);
            return orders;
        }

        public IQueryable<OrderData> GetOrders() {
            IQueryable<OrderData> orders = _ordersRepository.Queryable();
            loadType(orders);
            return orders;
        }

        private void loadType(IEnumerable<OrderData> orders) {
            //order.InvType = _eveDbContext.InvTypes.Find(order.TypeId);
            var enumerable = orders as IList<OrderData> ?? orders.ToList();
            var orderIds = enumerable.Select(f => f.TypeId);
            var types = _eveDbContext.InvTypes.Where(f => orderIds.Contains(f.TypeId));
            var lookup = types.ToLookup(f => f.TypeId);
            foreach (var order in enumerable) {
                order.InvType = lookup[order.TypeId].Single();
            }
        }

        private void loadType(OrderData orderData) {
            orderData.InvType = _eveDbContext.InvTypes.Find(orderData.TypeId);
        }

        public void AddOrders(IEnumerable<OrderData> orders) {
            foreach (OrderData order in orders) {
                if (!_ordersRepository.Queryable().Any(f => f.TypeId == order.TypeId))
                    _ordersRepository.Add(order);
            }
        }

        public void DeleteOrders(IEnumerable<OrderData> orders) {
            _ordersRepository.RemoveRange(orders);
        }

        public void SaveChanges() {
            _ordersRepository.SaveChanges();
        }

        public void LoadMarketData(ICollection<OrderData> orders, int dayLimit, int region = 10000002) {
            if (orders.Count == 0) return;
            var historyOptions = new EveMarketDataOptions();
            foreach (OrderData order in orders) {
                historyOptions.Items.Add(order.TypeId);
            }
            historyOptions.AgeSpan = TimeSpan.FromDays(dayLimit);
            historyOptions.Regions.Add(region);
            EveMarketDataResponse<ItemHistory> history = EveMarketData.GetItemHistory(historyOptions);
            ILookup<long, ItemHistory.ItemHistoryEntry> historyLookup = history.Result.History.ToLookup(f => f.TypeId);
            foreach (OrderData order in orders) {
                List<ItemHistory.ItemHistoryEntry> itemHistory = historyLookup[order.TypeId].ToList();
                if (!itemHistory.IsEmpty()) {
                    order.AvgPrice = itemHistory.Average(f => f.AvgPrice);
                    order.AvgVolume = itemHistory.Average(f => f.Volume);
                }
            }
        }

        public void SaveOrdersToDisk(string path, ICollection<OrderData> orders) {
            OrderInstallerIoService.WriteBuyOrders(path + Path.DirectorySeparatorChar + BuyOrdersFileName,
                ToBuyOrderCollection(orders));
            OrderInstallerIoService.WriteSellOrders(path + Path.DirectorySeparatorChar + SellOrdersFileName,
                ToSellOrderCollection(orders));
        }

        private static BuyOrderCollection ToBuyOrderCollection(IEnumerable<OrderData> orders) {
            var buyOrders = new BuyOrderCollection();
            foreach (OrderData order in orders.Where(order => order.IsBuyOrder)) {
                buyOrders.Add(ToBuyOrder(order));
            }
            return buyOrders;
        }

        private static SellOrderCollection ToSellOrderCollection(IEnumerable<OrderData> orders) {
            var sellOrders = new SellOrderCollection();
            foreach (OrderData order in orders.Where(order => order.IsSellOrder)) {
                sellOrders.Add(ToSellOrder(order));
            }
            return sellOrders;
        }


        public static SellOrder ToSellOrder(OrderData orderData) {
            var sellOrder = new SellOrder {
                TypeName = orderData.InvType.TypeName,
                TypeId = orderData.TypeId,
                MinPrice = (long) orderData.MinSellPrice,
                MaxQuantity = orderData.MaxSellQuantity,
                Quantity = orderData.MinSellQuantity,
                UpdateTime = DateTime.UtcNow,
            };
            return sellOrder;
        }

        public static BuyOrder ToBuyOrder(OrderData orderData) {
            var buyOrder = new BuyOrder {
                ItemName = orderData.InvType.TypeName,
                ItemId = orderData.TypeId,
                MaxPrice = (long) orderData.MaxBuyPrice,
                Quantity = orderData.BuyQuantity,
                //UpdateTime = DateTime.UtcNow,
            };
            return buyOrder;
        }

        public OrderData CreateOrder(BuyOrder buyOrder, SellOrder sellOrder) {
            var order = new OrderData();
            order.TypeId = sellOrder != null ? sellOrder.TypeId : buyOrder.ItemId;
            if (sellOrder != null) {
                order.IsSellOrder = true;
                order.MinSellPrice = sellOrder.MinPrice;
                order.MinSellQuantity = sellOrder.Quantity;
                order.MaxSellQuantity = sellOrder.MaxQuantity;
                order.UpdateTime = sellOrder.UpdateTime;
            }
            if (buyOrder != null) {
                order.IsBuyOrder = true;
                order.MaxBuyPrice = buyOrder.MaxPrice;
                order.BuyQuantity = buyOrder.Quantity;
            }
            return order;
        }

        public OrderData CreateOrder(int invTypeId) {
            var order = new OrderData();
            order.TypeId = invTypeId;
            loadType(order);
            return order;
        }
    }
}