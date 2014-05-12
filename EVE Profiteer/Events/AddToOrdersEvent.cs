using System.Collections.Generic;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Events {
    public class AddToOrdersEvent {
        public AddToOrdersEvent(IList<MarketAnalyzerItem> items) {
            Items = items;
        }

        public IList<MarketAnalyzerItem> Items { get; private set; }
    }
}
