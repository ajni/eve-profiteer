using System;
using System.Collections.Generic;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Events {
    public class AddToOrdersEventArgs : EventArgs {
        public AddToOrdersEventArgs(ICollection<InvType> items, bool sellOrder = true, bool buyOrder = true) {
            Items = items;
            SellOrder = sellOrder;
            BuyOrder = buyOrder;
        }

        public AddToOrdersEventArgs(InvType item, bool sellOrder = true, bool buyOrder = true) {
            Items = new List<InvType>();
            SellOrder = sellOrder;
            BuyOrder = buyOrder;
            Items.Add(item);
        }

        public bool SellOrder { get; private set; }

        public bool BuyOrder { get; private set; }

        public ICollection<InvType> Items { get; private set; }
    }
}