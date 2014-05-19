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
        private readonly IRepository<Order> _ordersRepository;
        public string BuyOrdersFileName = "BuyOrders.xml";

        public string SellOrdersFileName = "SellOrders.xml";


        public OrderEditorService(EveDbContext eveDbContext, IRepository<Order> ordersRepository) {
            _eveDbContext = eveDbContext;
            _ordersRepository = ordersRepository;
            EveMarketData = new EveMarketData();
        }

        private EveMarketData EveMarketData { get; set; }

        public ICollection<Order> LoadOrdersFromDisk(string path) {
            var orders = new List<Order>();
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

        public IQueryable<Order> GetOrders() {
            IQueryable<Order> orders = _ordersRepository.Queryable();
            loadType(orders);
            return orders;
        }

        private void loadType(IEnumerable<Order> orders) {
            //order.InvType = _eveDbContext.InvTypes.Find(order.TypeId);
            var enumerable = orders as IList<Order> ?? orders.ToList();
            var orderIds = enumerable.Select(f => f.TypeId);
            var types = _eveDbContext.InvTypes.Where(f => orderIds.Contains(f.TypeId));
            var lookup = types.ToLookup(f => f.TypeId);
            foreach (var order in enumerable) {
                order.InvType = lookup[order.TypeId].Single();
            }
        }

        private void loadType(Order order) {
            order.InvType = _eveDbContext.InvTypes.Find(order.TypeId);
        }

        public void AddOrders(IEnumerable<Order> orders) {
            foreach (Order order in orders) {
                if (!_ordersRepository.Queryable().Any(f => f.TypeId == order.TypeId))
                    _ordersRepository.Add(order);
            }
        }

        public void DeleteOrders(IEnumerable<Order> orders) {
            _ordersRepository.RemoveRange(orders);
        }

        public void SaveChanges() {
            _ordersRepository.SaveChanges();
        }

        public void LoadMarketData(ICollection<Order> orders, int dayLimit, int region = 10000002) {
            if (orders.Count == 0) return;
            var historyOptions = new EveMarketDataOptions();
            foreach (Order order in orders) {
                historyOptions.Items.Add(order.TypeId);
            }
            historyOptions.AgeSpan = TimeSpan.FromDays(dayLimit);
            historyOptions.Regions.Add(region);
            EveMarketDataResponse<ItemHistory> history = EveMarketData.GetItemHistory(historyOptions);
            ILookup<long, ItemHistory.ItemHistoryEntry> historyLookup = history.Result.History.ToLookup(f => f.TypeId);
            foreach (Order order in orders) {
                List<ItemHistory.ItemHistoryEntry> itemHistory = historyLookup[order.TypeId].ToList();
                if (!itemHistory.IsEmpty()) {
                    order.AvgPrice = itemHistory.Average(f => f.AvgPrice);
                    order.AvgVolume = itemHistory.Average(f => f.Volume);
                }
            }
        }

        public void SaveOrdersToDisk(string path, ICollection<Order> orders) {
            OrderInstallerIoService.WriteBuyOrders(path + Path.DirectorySeparatorChar + BuyOrdersFileName,
                ToBuyOrderCollection(orders));
            OrderInstallerIoService.WriteSellOrders(path + Path.DirectorySeparatorChar + SellOrdersFileName,
                ToSellOrderCollection(orders));
        }

        private static BuyOrderCollection ToBuyOrderCollection(IEnumerable<Order> orders) {
            var buyOrders = new BuyOrderCollection();
            foreach (Order order in orders.Where(order => order.IsBuyOrder)) {
                buyOrders.Add(ToBuyOrder(order));
            }
            return buyOrders;
        }

        private static SellOrderCollection ToSellOrderCollection(IEnumerable<Order> orders) {
            var sellOrders = new SellOrderCollection();
            foreach (Order order in orders.Where(order => order.IsSellOrder)) {
                sellOrders.Add(ToSellOrder(order));
            }
            return sellOrders;
        }


        public static SellOrder ToSellOrder(Order order) {
            var sellOrder = new SellOrder {
                TypeName = order.InvType.TypeName,
                TypeId = order.TypeId,
                MinPrice = (long) order.MinSellPrice,
                MaxQuantity = order.MaxSellQuantity,
                Quantity = order.MinSellQuantity,
                UpdateTime = DateTime.UtcNow,
            };
            return sellOrder;
        }

        public static BuyOrder ToBuyOrder(Order order) {
            var buyOrder = new BuyOrder {
                ItemName = order.InvType.TypeName,
                ItemId = order.TypeId,
                MaxPrice = (long) order.MaxBuyPrice,
                Quantity = order.BuyQuantity,
                //UpdateTime = DateTime.UtcNow,
            };
            return buyOrder;
        }

        public Order CreateOrder(BuyOrder buyOrder, SellOrder sellOrder) {
            var order = new Order();
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

        public Order CreateOrder(int invTypeId) {
            var order = new Order();
            order.TypeId = invTypeId;
            loadType(order);
            return order;
        }
    }
}