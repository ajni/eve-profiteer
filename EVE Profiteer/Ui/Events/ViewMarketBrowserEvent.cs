using System;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.ViewModels.Modules;

namespace eZet.EveProfiteer.Ui.Events {
    public class ViewMarketBrowserEvent : ModuleEvent {
        public ViewMarketBrowserEvent(InvType invType) {
            InvType = invType;
            Focus = true;
        }

        public InvType InvType { get; private set; }
        public override Type GetTabType() {
            return typeof (MarketBrowserViewModel);
        }

    }
}