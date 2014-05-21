using System.Collections.Generic;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Events {
    public class OrdersAddedEventArgs {
        public OrdersAddedEventArgs(List<Order> orders) {
            Orders = orders;
        }

        public ICollection<Order> Orders { get; private set; }
    }
}