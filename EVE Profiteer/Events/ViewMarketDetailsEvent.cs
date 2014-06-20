using System;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Events {
    public class ViewMarketDetailsEvent : IActivateTabEvent {
        public ViewMarketDetailsEvent(InvType invType) {
            InvType = invType;
        }

        public InvType InvType { get; private set; }
        public Type GetTabType() {
            return typeof (ViewMarketDetailsEvent);
        }
    }
}