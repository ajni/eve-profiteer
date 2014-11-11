using System;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Ui.Events;
using eZet.EveProfiteer.ViewModels.Modules;

namespace eZet.EveProfiteer.Events {
    public class ViewOrderEvent : ModuleEvent {
        public ViewOrderEvent(InvType invType) {
            InvType = invType;
        }

        public InvType InvType { get; set; }
        public override Type GetTabType() {
            return typeof(OrderManagerViewModel);
        }
    }
}