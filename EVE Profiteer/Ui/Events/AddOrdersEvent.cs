using System;
using System.Collections.Generic;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Events {
    public class AddOrdersEvent : EventArgs {
        public AddOrdersEvent(ICollection<InvType> items, bool sellOrder = true, bool buyOrder = true) {
            Items = items;
            SellOrder = sellOrder;
            BuyOrder = buyOrder;
        }

        public AddOrdersEvent(InvType item, bool sellOrder = true, bool buyOrder = true) {
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