using System;
using System.Collections.Generic;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Events {
    public class AddToOrdersEventArgs : EventArgs{
        public AddToOrdersEventArgs(ICollection<InvType> items) {
            Items = items;
        }
        public AddToOrdersEventArgs(InvType item) {
            Items = new List<InvType>();
            Items.Add(item);
        }

        public ICollection<InvType> Items { get; private set; }
    }
}