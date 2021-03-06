﻿using System;

namespace eZet.EveProfiteer.Models {
    public class MarketHistoryAggregateEntry {
        public long Volume { get; set; }

        public long OrderCount { get; set; }

        public decimal LowPrice { get; set; }

        public decimal HighPrice { get; set; }

        public decimal AvgPrice { get; set; }

        public DateTime Date { get; set; }

        public decimal DonchianHigh { get; set; }

        public decimal DonchianLow { get; set; }

        public decimal DonchianCenter { get; set; }

        public decimal Moving20DayAvg { get; set; }

        public decimal Moving5DayAvg { get; set; }

        public double CommodityChannelIndex { get; set; }
    }
}