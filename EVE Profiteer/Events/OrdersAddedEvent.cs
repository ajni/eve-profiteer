using System.Collections.Generic;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Events {
    public class OrdersAddedEvent {
        public OrdersAddedEvent(List<Order> orders) {
            Orders = orders;
        }

        public ICollection<Order> Orders { get; private set; }
    }
}