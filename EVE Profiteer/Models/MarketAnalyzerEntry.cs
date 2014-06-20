using System;
using System.Collections.Generic;
using System.Linq;
using eZet.EveLib.Modules.Models;

namespace eZet.EveProfiteer.Models {
    public class MarketAnalyzerEntry {
        private const int StandardDeviationFactor = 1;
        public IEnumerable<EmdItemHistory.ItemHistoryEntry> History;

        public MarketAnalyzerEntry(InvType invType, EmdItemPrices.ItemPriceEntry sellOrder,
            EmdItemPrices.ItemPriceEntry buyOrder, IEnumerable<EmdItemHistory.ItemHistoryEntry> history) {
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
        public decimal DailyGrossProfit { get; set; }

        public decimal GrossProfitPerUnit { get; set; }

        public decimal SellPrice { get; set; }

        public decimal BuyPrice { get; set; }

        public decimal GrossMargin { get; set; }

        public double SellPriceIndex { get; set; }

        public double BuyPriceIndex { get; set; }

        #endregion

        #region AVERAGE
        public decimal AvgDailyGrossProfit { get; private set; }

        public decimal AvgGrossProfitPerUnit { get; private set; }

        public decimal AvgPrice { get; private set; }

        public decimal AvgMinPrice { get; private set; }

        public decimal AvgMaxPrice { get; private set; }

        public double AvgGrossMargin { get; private set; }


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
            GrossProfitPerUnit = SellPrice - BuyPrice;
            GrossMargin = SellPrice != 0 ? GrossProfitPerUnit / SellPrice : 0;
            if (!History.Any()) return;
            List<EmdItemHistory.ItemHistoryEntry> historyByVolume = History.OrderBy(f => f.Volume).ToList();
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
            DailyGrossProfit = GrossProfitPerUnit * (decimal)VolumeMedian;
            List<EmdItemHistory.ItemHistoryEntry> historyByDate = History.OrderBy(f => f.Date).ToList();
            foreach (EmdItemHistory.ItemHistoryEntry item in historyByDate) {
                AvgPrice += item.AvgPrice;
                AvgMinPrice += item.MinPrice;
                AvgMaxPrice += item.MaxPrice;
            }
            AvgPrice /= History.Count();
            AvgMaxPrice /= History.Count();
            AvgMinPrice /= History.Count();
            AvgGrossProfitPerUnit = AvgMaxPrice - AvgMinPrice;
            if (AvgMaxPrice > 0)
                AvgGrossMargin = (double)(AvgGrossProfitPerUnit / AvgMaxPrice);
            AvgDailyGrossProfit = AvgGrossProfitPerUnit * (decimal)VolumeMedian;
            SellPriceIndex = AvgMaxPrice != 0 ? (double)((SellPrice / AvgMaxPrice) - 1) * 100 : 0;
            BuyPriceIndex = BuyPrice != 0 ? (double)((AvgMinPrice / BuyPrice) - 1) * 100 : 0;
        }

        private bool isOutlier(double volume) {
            return volume > VolumeMedian + VolumeStandardDeviation * StandardDeviationFactor ||
                   volume < VolumeMedian - VolumeStandardDeviation * StandardDeviationFactor;
        }
    }
}