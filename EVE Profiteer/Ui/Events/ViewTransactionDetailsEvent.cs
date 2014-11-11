using System;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.ViewModels.Modules;

namespace eZet.EveProfiteer.Ui.Events {
    public class ViewTransactionDetailsEvent : ModuleEvent {
        public ViewTransactionDetailsEvent(InvType invType) {
            InvType = invType;
        }

        public InvType InvType { get; private set; }

        public override Type GetTabType() {
            return typeof (TransactionDetailsViewModel);
        }
    }
}