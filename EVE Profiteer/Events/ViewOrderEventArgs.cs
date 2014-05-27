using System;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Events {
    public class ViewOrderEventArgs : EventArgs {
        public ViewOrderEventArgs(InvType invType) {
            InvType = invType;
        }

        public InvType InvType { get; set; }
    }
}