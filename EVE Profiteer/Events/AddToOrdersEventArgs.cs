using System.Collections.Generic;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Events {
    public class AddToOrdersEventArgs {
        public AddToOrdersEventArgs(IList<MarketAnalyzerEntry> items) {
            Items = items;
        }

        public IList<MarketAnalyzerEntry> Items { get; private set; }
    }
}