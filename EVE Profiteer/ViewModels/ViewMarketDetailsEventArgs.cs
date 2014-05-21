using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.ViewModels {
    public class ViewMarketDetailsEventArgs {

        public InvType InvType { get; private set; }

        public ViewMarketDetailsEventArgs(InvType invType) {
            InvType = invType;
        }
    }
}
