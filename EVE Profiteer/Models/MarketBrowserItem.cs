using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace eZet.EveProfiteer.Models {
    public class MarketBrowserItem {
        public MarketBrowserItem(InvType invType, IEnumerable<MarketHistoryAggregateEntry> marketHistory,
            IEnumerable<MarketBrowserOrder> sellOrders, IEnumerable<MarketBrowserOrder> buyOrders, int donchianLength) {
            InvType = invType;
            MarketHistory = marketHistory.OrderBy(entry => entry.Date).ToList();
            BuyOrders = buyOrders.OrderByDescending(t => t.Price).ToList();
            SellOrders = sellOrders.OrderBy(t => t.Price).ToList();
            DonchianLength = donchianLength;
            initialize();
        }

        public string CurrentBuyPriceString {
            get { return "Buy: " + CurrentBuyPrice.ToString("N2"); }
        }

        public string CurrentSellPriceString {
            get { return "Sell: " + CurrentSellPrice.ToString("N2"); }
        }

        public int DonchianLength { get; set; }

        public double CommodityChannelIndexFactor = 0.015;

        public InvType InvType { get; set; }
        public ICollection<MarketHistoryAggregateEntry> MarketHistory { get; set; }

        public ICollection<MarketBrowserOrder> SellOrders { get; private set; }

        public ICollection<MarketBrowserOrder> BuyOrders { get; private set; }

        public decimal CurrentBuyPrice { get; private set; }

        public decimal CurrentSellPrice { get; private set; }

        private void initialize() {
            if (BuyOrders.Any()) {
                CurrentBuyPrice = BuyOrders.MaxBy(order => order.Price).Price;
            }
            if (SellOrders.Any()) {
                CurrentSellPrice = SellOrders.MinBy(order => order.Price).Price;
            }
            var high = new List<decimal>();
            var low = new List<decimal>();
            if (!MarketHistory.Any()) {
                return;
            }
            foreach (MarketHistoryAggregateEntry entry in MarketHistory) {
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
                entry.DonchianCenter = (entry.DonchianHigh + entry.DonchianLow) / 2;
            }
        }

    }
}