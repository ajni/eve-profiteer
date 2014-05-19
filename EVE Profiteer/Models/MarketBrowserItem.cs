using eZet.EveOnlineDbModels;

namespace eZet.EveProfiteer.Models {
    public class MarketBrowserItem {
        public MarketBrowserItem(InvType invType) {
            InvType = invType;
        }

        public InvType InvType { get; set; }
    }
}