using System;
using System.Collections.Generic;
using System.Linq;
using eZet.EveLib.Modules.Models;
using eZet.EveOnlineDbModels;

namespace eZet.EveProfiteer.Models {
    public class MarketAnalyzerEntry {
        private const int StandardDeviationFactor = 1;
        public IEnumerable<ItemHistory.ItemHistoryEntry> History;

        public MarketAnalyzerEntry(InvType invType, ItemPrices.ItemPriceEntry sellOrder,
            ItemPrices.ItemPriceEntry buyOrder, IEnumerable<ItemHistory.ItemHistoryEntry> history) {
            InvType = invType;
            History = history;
            Updated = sellOrder.Updated;
            BuyPrice = buyOrder.Price;
            SellPrice = sellOrder.Price;
            Calculate();
        }

        public InvType InvType { get; set; }

        public string OrderType { get; set; }

        public decimal SellPrice { get; set; }

        public decimal BuyPrice { get; set; }

        public double DailyVolumeAverage { get; set; }

        public double DailyVolumeAdjustedAverage { get; set; }

        public double DailyVolumeMedian { get; set; }

        public decimal ProfitPerItem { get; set; }

        public decimal DailyProfit { get; set; }

        public decimal Margin { get; set; }

        public double VolumeStandardDeviation { get; set; }

        public double VolumeVariance { get; set; }

        public string Updated { get; set; }

        public Order Order { get; set; }

        public void Calculate() {
            ProfitPerItem = SellPrice - BuyPrice;
            Margin = BuyPrice != 0 ? ProfitPerItem/BuyPrice : 0;
            if (!History.Any()) return;
            List<ItemHistory.ItemHistoryEntry> list = History.OrderBy(f => f.Volume).ToList();
            DailyVolumeMedian = list.Count == 1 ? list[0].Volume : list[list.Count/2 - 1].Volume;
            DailyVolumeAverage = History.Average(f => f.Volume);
            if (History.Count()%2 == 0) {
                DailyVolumeMedian = (DailyVolumeMedian + list[list.Count/2].Volume)/2;
            }
            var variance = new List<double>();
            list.ForEach(f => variance.Add(Math.Pow(f.Volume - DailyVolumeAverage, 2)));
            VolumeVariance = variance.Average();
            VolumeStandardDeviation = Math.Sqrt(VolumeVariance);
            DailyVolumeAdjustedAverage = History.Where(f => !isOutlier(f.Volume)).Average(f => f.Volume);
            DailyProfit = ProfitPerItem*(decimal) DailyVolumeMedian;
        }

        private bool isOutlier(double volume) {
            return volume > DailyVolumeMedian + VolumeStandardDeviation*StandardDeviationFactor ||
                   volume < DailyVolumeMedian - VolumeStandardDeviation*StandardDeviationFactor;
        }
    }
}