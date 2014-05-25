using System;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Events {
    public class ViewOrderEventArgs : EventArgs {
        public InvType InvType { get; set; }

        public ViewOrderEventArgs(InvType invType) {
            InvType = invType;
        }
    }
}
