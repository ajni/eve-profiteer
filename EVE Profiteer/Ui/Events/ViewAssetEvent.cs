using System;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.ViewModels.Modules;

namespace eZet.EveProfiteer.Ui.Events {
    public class ViewAssetEvent : ModuleEvent {
        public InvType InvType { get; private set; }

        public ViewAssetEvent(InvType invType) {
            InvType = invType;
            Focus = true;
        }

        public override Type GetTabType() {
            return typeof(AssetsViewModel);
        }
    }
}
