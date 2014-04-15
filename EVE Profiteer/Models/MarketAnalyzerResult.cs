using System;
using System.Collections.Generic;
using System.Linq;
using eZet.EveLib.Modules.Models;

namespace eZet.EveProfiteer.Models {
    public class MarketAnalyzerResult {

        public List<ItemHistory.ItemHistoryEntry> Data { get; private set; }

        public MarketAnalyzerResult(Region region, Item item) {
            TypeId = item.TypeId;
            ItemName = item.TypeName;
            RegionId = region.RegionId;
            RegionName = region.RegionName;
            Data = new List<ItemHistory.ItemHistoryEntry>();
        }

        public bool IsChecked { get; set; }

        public double TotalRating { get; private set; }

        public long TypeId { get; private set; }

        public long RegionId { get; private set; }

        public string ItemName { get; private set; }

        public string RegionName { get; private set; }

        public double AvgVolume { get; private set; }

        public decimal AvgPrice { get; private set; }

        public decimal AvgBuyPrice { get; private set; }

        public decimal AvgSellPrice { get; private set; }

        public decimal ProfitPerItem { get; private set; }

        public decimal DailyProfit { get; private set; }

        public double Margin { get; private set; }

        public void Calculate() {
            AvgVolume = Data.Sum(t => t.Volume) / (double)Data.Count;
            AvgPrice = Data.Sum(t => t.AvgPrice) / Data.Count;
            AvgBuyPrice = Data.Sum(t => t.MinPrice) / Data.Count;
            AvgSellPrice = Data.Sum(t => t.MaxPrice) / Data.Count;
            ProfitPerItem = AvgSellPrice - AvgBuyPrice;
            Margin = (double)(ProfitPerItem / AvgBuyPrice);
            DailyProfit = ProfitPerItem * (decimal)AvgVolume;
            calculateProfitRating();
        }

        private const double VolumeWeight = 0.2;
        private const double MarginWeight = 0.2;
        private const double ProfitWeight = 1 - VolumeWeight - MarginWeight;
        private const int VolumeCutoff = 10;
        private const double MarginCutoff = 0.4;
        private const double TargetRating = 1000;

        private void calculateProfitRating() {
            var volRating = TargetRating * (AvgVolume / (VolumeCutoff+1));
            if (AvgVolume > VolumeCutoff)
                volRating = Math.Pow(-(0.008 * AvgVolume - 4), 2)  + TargetRating;

            var marginRating = TargetRating * (Margin / MarginCutoff);
            if (Margin > MarginCutoff)
                marginRating = Math.Pow(-(0.1 * Margin * 100 - 5), 2) + TargetRating;

            var profitRating = (double)DailyProfit / 50000000 * TargetRating; // 50M 


            TotalRating = (volRating*VolumeWeight) * (marginRating*MarginWeight) * (profitRating*ProfitWeight) * TargetRating / (200 * 200 * 600);
        }

    }
}
