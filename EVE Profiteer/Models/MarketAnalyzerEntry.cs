using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Utils.Drawing;
using eZet.EveLib.Modules.Models;

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

        public double VolumeAverage { get; set; }

        public double VolumeAdjustedAverage { get; set; }

        public double VolumeMedian { get; set; }

        public decimal ProfitPerItem { get; set; }

        public decimal DailyProfit { get; set; }

        public decimal Margin { get; set; }

        public double VolumeStandardDeviation { get; set; }

        public double VolumeVariance { get; set; }

        public string Updated { get; set; }

        public Order Order { get; set; }

        public decimal AvgPrice { get; private set; }

        public decimal AvgMinPrice { get; private set; }

        public decimal AvgMaxPrice { get; private set; }

        public decimal AvgProfitPerItem { get; private set; }

        public double AvgMargin { get; private set; }

        public decimal AvgDailyProfit { get; private set; }

        public void Calculate() {
            ProfitPerItem = SellPrice - BuyPrice;
            Margin = BuyPrice != 0 ? ProfitPerItem / BuyPrice : 0;
            if (!History.Any()) return;
            List<ItemHistory.ItemHistoryEntry> historyByVolume = History.OrderBy(f => f.Volume).ToList();
            VolumeMedian = historyByVolume.Count == 1 ? historyByVolume[0].Volume : historyByVolume[historyByVolume.Count / 2 - 1].Volume;
            VolumeAverage = History.Average(f => f.Volume);
            if (History.Count() % 2 == 0) {
                VolumeMedian = (VolumeMedian + historyByVolume[historyByVolume.Count / 2].Volume) / 2;
            }
            var variance = new List<double>();
            historyByVolume.ForEach(f => variance.Add(Math.Pow(f.Volume - VolumeAverage, 2)));
            VolumeVariance = variance.Average();
            VolumeStandardDeviation = Math.Sqrt(VolumeVariance);
            VolumeAdjustedAverage = History.Where(f => !isOutlier(f.Volume)).Average(f => f.Volume);
            DailyProfit = ProfitPerItem * (decimal)VolumeMedian;
            List<ItemHistory.ItemHistoryEntry> historyByDate = History.OrderBy(f => f.Date).ToList();
            foreach (var item in historyByDate) {
                AvgPrice += item.AvgPrice;
                AvgMinPrice += item.MinPrice;
                AvgMaxPrice += item.MaxPrice;
            }
            AvgPrice /= History.Count();
            AvgMaxPrice /= History.Count();
            AvgMinPrice /= History.Count();
            AvgProfitPerItem = AvgMaxPrice - AvgMinPrice;
            if (AvgMaxPrice > 0)
                AvgMargin = (double)(AvgProfitPerItem / AvgMaxPrice);
            AvgDailyProfit = AvgProfitPerItem * (decimal)VolumeMedian;

        }

        private bool isOutlier(double volume) {
            return volume > VolumeMedian + VolumeStandardDeviation * StandardDeviationFactor ||
                   volume < VolumeMedian - VolumeStandardDeviation * StandardDeviationFactor;
        }
    }
}