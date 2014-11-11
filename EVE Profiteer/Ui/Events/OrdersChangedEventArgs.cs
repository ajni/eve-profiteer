using System;
using System.Collections.Generic;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Ui.Events {
    public class OrdersChangedEventArgs : EventArgs {

        public ICollection<Order> Added { get; set; }
        public ICollection<Order> Removed { get; set; }
        public ICollection<Order> Changed { get; set; }
    }
}