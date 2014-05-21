using System.Collections.Generic;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Events {
    public class OrdersAddedEventArgs {
        public OrdersAddedEventArgs(List<OrderData> orders) {
            Orders = orders;
        }

        public ICollection<OrderData> Orders { get; private set; }
    }
}