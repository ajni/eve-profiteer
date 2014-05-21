using System.Collections.Generic;
using System.Linq;

namespace eZet.EveProfiteer.Models {
    public class MarketBrowserItem {
        public MarketBrowserItem(InvType invType, IEnumerable<MarketHistoryEntry> marketHistory, ICollection<MarketOrder> buyOrders, ICollection<MarketOrder> sellOrders, int donchianLength) {
            InvType = invType;
            MarketHistory = marketHistory.OrderBy(entry => entry.Date).ToList();
            BuyOrders = buyOrders;
            SellOrders = sellOrders;
            DonchianLength = donchianLength;
            initialize();
        }

        public int DonchianLength { get; set; }

        public InvType InvType { get; set; }
        public ICollection<MarketHistoryEntry> MarketHistory { get; set; }

        public ICollection<MarketOrder> SellOrders { get; private set; }

        public ICollection<MarketOrder> BuyOrders { get; private set; }

        private void initialize() {
            var high = new List<decimal>();
            var low = new List<decimal>();
            foreach (var entry in MarketHistory) {
                high.Add(entry.HighPrice);
                low.Add(entry.LowPrice);
                if (high.Count > DonchianLength)
                    high.RemoveAt(0);
                if (low.Count > DonchianLength)
                    low.RemoveAt(0);
                if (high.Any())
                    entry.DonchianHigh = high.Max();
                if (low.Any())
                    entry.DonchianLow = low.Min();
                entry.DonchianCenter = (entry.DonchianHigh + entry.DonchianLow)/2;
            }


        }
    }
}