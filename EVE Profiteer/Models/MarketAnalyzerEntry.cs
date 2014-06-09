using System;
using System.Collections.Generic;
using System.Linq;
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

        public Order Order { get; set; }

        //public string OrderType { get; set; }

        #region CURRENT
        public decimal DailyProfit { get; set; }

        public decimal ProfitPerUnit { get; set; }

        public decimal SellPrice { get; set; }

        public decimal BuyPrice { get; set; }

        public decimal Margin { get; set; }

        public double SellPriceIndex { get; set; }

        public double BuyPriceIndex { get; set; }

        #endregion

        #region AVERAGE
        public decimal AvgDailyProfit { get; private set; }

        public decimal AvgProfitPerUnit { get; private set; }

        public decimal AvgPrice { get; private set; }

        public decimal AvgMinPrice { get; private set; }

        public decimal AvgMaxPrice { get; private set; }

        public double AvgMargin { get; private set; }


        #endregion

        #region VOLUME

        public double VolumeAverage { get; set; }

        public double VolumeAdjustedAverage { get; set; }

        public double VolumeMedian { get; set; }


        public double VolumeStandardDeviation { get; set; }

        public double VolumeVariance { get; set; }

        #endregion

        public DateTime Updated { get; set; }


        public void Calculate() {
            ProfitPerUnit = SellPrice - BuyPrice;
            Margin = SellPrice != 0 ? ProfitPerUnit / SellPrice : 0;
            if (!History.Any()) return;
            List<ItemHistory.ItemHistoryEntry> historyByVolume = History.OrderBy(f => f.Volume).ToList();
            VolumeMedian = historyByVolume.Count == 1
                ? historyByVolume[0].Volume
                : historyByVolume[historyByVolume.Count / 2].Volume;
            if (History.Count() % 2 == 0) {
                VolumeMedian = (VolumeMedian + historyByVolume[historyByVolume.Count / 2 - 1].Volume) / 2;
            }
            VolumeAverage = History.Average(f => f.Volume);
            var variance = new List<double>();
            historyByVolume.ForEach(f => variance.Add(Math.Pow(f.Volume - VolumeAverage, 2)));
            VolumeVariance = variance.Average();
            VolumeStandardDeviation = Math.Sqrt(VolumeVariance);
            VolumeAdjustedAverage = History.Where(f => !isOutlier(f.Volume)).Average(f => f.Volume);
            DailyProfit = ProfitPerUnit * (decimal)VolumeMedian;
            List<ItemHistory.ItemHistoryEntry> historyByDate = History.OrderBy(f => f.Date).ToList();
            foreach (ItemHistory.ItemHistoryEntry item in historyByDate) {
                AvgPrice += item.AvgPrice;
                AvgMinPrice += item.MinPrice;
                AvgMaxPrice += item.MaxPrice;
            }
            AvgPrice /= History.Count();
            AvgMaxPrice /= History.Count();
            AvgMinPrice /= History.Count();
            AvgProfitPerUnit = AvgMaxPrice - AvgMinPrice;
            if (AvgMaxPrice > 0)
                AvgMargin = (double)(AvgProfitPerUnit / AvgMaxPrice);
            AvgDailyProfit = AvgProfitPerUnit * (decimal)VolumeMedian;
            SellPriceIndex = AvgMaxPrice != 0 ? (double)((SellPrice / AvgMaxPrice) - 1) * 100 : 0;
            BuyPriceIndex = BuyPrice != 0 ? (double)((AvgMinPrice / BuyPrice) - 1) * 100 : 0;
        }

        private bool isOutlier(double volume) {
            return volume > VolumeMedian + VolumeStandardDeviation * StandardDeviationFactor ||
                   volume < VolumeMedian - VolumeStandardDeviation * StandardDeviationFactor;
        }
    }
}