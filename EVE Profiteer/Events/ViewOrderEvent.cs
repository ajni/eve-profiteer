using System;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.ViewModels.Tabs;

namespace eZet.EveProfiteer.Events {
    public class ViewOrderEvent : IActivateTabEvent {
        public ViewOrderEvent(InvType invType) {
            InvType = invType;
        }

        public InvType InvType { get; set; }
        public Type GetTabType() {
            return typeof(OrderEditorViewModel);
        }
    }
}