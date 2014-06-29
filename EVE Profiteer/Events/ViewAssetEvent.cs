using System;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.ViewModels.Tabs;

namespace eZet.EveProfiteer.Events {
    public class ViewAssetEvent : IActivateTabEvent {
        public InvType InvType { get; private set; }

        public ViewAssetEvent(InvType invType) {
            InvType = invType;
        }

        public Type GetTabType() {
            return typeof(AssetsViewModel);
        }


    }
}
