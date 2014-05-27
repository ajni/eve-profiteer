using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.ViewModels {
    public class ViewMarketDetailsEventArgs {
        public ViewMarketDetailsEventArgs(InvType invType) {
            InvType = invType;
        }

        public InvType InvType { get; private set; }
    }
}