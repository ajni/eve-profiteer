using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Events {
    public class ViewTradeDetailsEventArgs {
        public InvType InvType { get; private set; }

        public ViewTradeDetailsEventArgs(InvType invType) {
            InvType = invType;
        }
    }
}
