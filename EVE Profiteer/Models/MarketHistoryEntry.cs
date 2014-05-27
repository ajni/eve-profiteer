using System;

namespace eZet.EveProfiteer.Models {
    public class MarketHistoryEntry {
        public int Volume { get; set; }

        public int OrderCount { get; set; }

        public decimal LowPrice { get; set; }

        public decimal HighPrice { get; set; }

        public decimal AvgPrice { get; set; }

        public DateTime Date { get; set; }

        public decimal DonchianHigh { get; set; }

        public decimal DonchianLow { get; set; }

        public decimal DonchianCenter { get; set; }
    }
}