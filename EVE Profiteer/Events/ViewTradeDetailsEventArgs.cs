using System;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Events {
    public class ViewTradeDetailsEventArgs : EventArgs {
        public InvType InvType { get; private set; }

        public ViewTradeDetailsEventArgs(InvType invType) {
            InvType = invType;
        }
    }
}
