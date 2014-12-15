using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using eZet.Eve.OrderXmlHelper;
using eZet.Eve.OrderXmlHelper.Models;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Services {
    public class OrderXmlService {

        public string BuyOrdersFileName = "BuyOrders.xml";

        public string SellOrdersFileName = "SellOrders.xml";


        public OrderXmlService() {
        }

        public ICollection<Order> ImportOrders(string path) {
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
            } catch (FileNotFoundException) {
            }
            return orders;
        }

        public void ExportOrders(string path, IEnumerable<Order> enumerable) {
            var orders = enumerable as IList<Order> ?? enumerable.ToList();
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


        private static SellOrder ToSellOrder(Order order) {
            var sellOrder = new SellOrder {
                TypeName = order.InvType.TypeName,
                TypeId = order.TypeId,
                MinPrice = (long)order.MinSellPrice,
                MaxQuantity = order.MaxSellQuantity,
                Quantity = order.MinSellQuantity,
                UpdateTime = DateTime.UtcNow,
            };
            return sellOrder;
        }

        private static BuyOrder ToBuyOrder(Order order) {
            var buyOrder = new BuyOrder {
                ItemName = order.InvType.TypeName,
                ItemId = order.TypeId,
                MaxPrice = (long)order.MaxBuyPrice,
                Quantity = order.BuyQuantity,
                //UpdateTime = DateTime.UtcNow,
            };
            return buyOrder;
        }

        private static Order CreateOrder(BuyOrder buyOrder, SellOrder sellOrder) {
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
            order.AutoProcess = true;
            return order;
        }
    }
}