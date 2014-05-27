using System;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Events {
    public class ViewTradeDetailsEventArgs : EventArgs {
        public ViewTradeDetailsEventArgs(InvType invType) {
            InvType = invType;
        }

        public InvType InvType { get; private set; }
    }
}