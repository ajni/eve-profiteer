using System;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.ViewModels.Tabs;

namespace eZet.EveProfiteer.Events {
    public class ViewMarketBrowserEvent : IActivateTabEvent {
        public ViewMarketBrowserEvent(InvType invType) {
            InvType = invType;
        }

        public InvType InvType { get; private set; }
        public Type GetTabType() {
            return typeof (MarketBrowserViewModel);
        }
    }
}