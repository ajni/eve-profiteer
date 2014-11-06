using System;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Ui.Events;
using eZet.EveProfiteer.ViewModels.Modules;

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
