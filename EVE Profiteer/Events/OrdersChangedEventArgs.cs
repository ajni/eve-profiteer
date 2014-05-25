using System;
using System.Collections.Generic;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Events {
    public class OrdersChangedEventArgs : EventArgs {
        public OrdersChangedEventArgs(List<Order> orders) {
            Orders = orders;
        }

        public ICollection<Order> Orders { get; private set; }
    }
}