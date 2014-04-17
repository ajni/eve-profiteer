using System.Collections.ObjectModel;
using System.Linq;

namespace eZet.Eve.OrderIoHelper.Models {
    public class OrderCollection : Collection<Order> {

        public OrderCollection(BuyOrderCollection buyOrders, SellOrderCollection sellOrders) {
            var sellOrderLookup = sellOrders.ToLookup(f => f.ItemId);
            foreach (var buyOrder in buyOrders) {
                var sellOrder = sellOrderLookup[buyOrder.ItemId].SingleOrDefault();
                sellOrders.Remove(sellOrder);
                Items.Add(new Order(buyOrder, sellOrder));
            }
            foreach (var sellOrder in sellOrders) {
                Items.Add(new Order(null, sellOrder));
            }
        }

        public BuyOrderCollection ToBuyOrderCollection() {
            var orders = new BuyOrderCollection();
            foreach (var order in this) {
                if (order.BuyQuantity > 0)
                    orders.Add(order.ToBuyOrder());
            }
            return orders;
        }

        public SellOrderCollection ToSellOrderCollection() {
            var orders = new SellOrderCollection();
            foreach (var order in this) {
                if (order.MinSellQuantity > 0)
                    orders.Add(order.ToSellOrder());
            }
            return orders;
        }
    }
}
