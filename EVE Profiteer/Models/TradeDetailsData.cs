using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace eZet.EveProfiteer.Models {
    public class TradeDetailsData {
        public TradeDetailsData(InvType invType, IEnumerable<Transaction> transactions, Order order) {
            InvType = invType;
            Transactions = transactions;
            Order = order;
            ChartEntries = new List<TradeDetailsChartPoint>();
            initialize();
        }

        public InvType InvType { get; set; }
        public IEnumerable<Transaction> Transactions { get; set; }
        public Order Order { get; set; }

        public ICollection<TradeDetailsChartPoint> ChartEntries { get; set; }

        public int SellQuantity { get; private set; }

        public int BuyQuantity { get; private set; }

        public DateTime FirstTrade { get; private set; }

        public DateTime LastTrade { get; private set; }

        public decimal BuyTotal { get; private set; }

        public decimal SellTotal { get; private set; }

        public decimal AvgBuyPrice { get; private set; }

        public decimal AvgSellPrice { get; private set; }

        public decimal MinBuyPrice { get; private set; }

        public decimal MaxBuyPrice { get; private set; }

        public decimal MinSellPrice { get; private set; }

        public decimal MaxSellPrice { get; private set; }

        public decimal PeriodicAverageProfit { get; private set; }

        public decimal PerpetualAverageProfit { get; private set; }

        public decimal PerpetualAverageTotalCost { get; private set; }

        public decimal ProfitPerItem { get; private set; }

        public double AvgMargin { get; private set; }

        public decimal Balance { get; private set; }

        public int Stock { get; private set; }

        public int UnaccountedStock { get; private set; }

        public decimal StockValue { get; private set; }

        public decimal AvgProfitPerDay { get; private set; }
        public decimal PerpetualAverageCost { get; private set; }

        public TimeSpan TradeDuration { get; private set; }

        private void initialize() {
            var groups = Transactions.GroupBy(f => f.TransactionDate.Date).Select(f => f.ToList()).ToList();
            FirstTrade = DateTime.MaxValue;
            LastTrade = DateTime.MinValue;
            foreach (var transactions in groups) {
                var entry = new TradeDetailsChartPoint(transactions.First().TransactionDate.Date, transactions);
                ChartEntries.Add(entry);
                Balance += entry.Balance;
                SellQuantity += entry.SellQuantity;
                BuyQuantity += entry.BuyQuantity;
                BuyTotal += entry.BuyTotal;
                SellTotal += entry.SellTotal;
                PerpetualAverageTotalCost += entry.PerpetualAverageTotalCost;
                UnaccountedStock += entry.UnaccountedStock;
                if (entry.Date < FirstTrade)
                    FirstTrade = entry.Date;
                if (entry.Date > LastTrade)
                    LastTrade = entry.Date;
            }

            TradeDuration = LastTrade - FirstTrade;

            if (BuyQuantity > 0)
                AvgBuyPrice = BuyTotal / BuyQuantity;
            if (SellQuantity > 0)
                AvgSellPrice = SellTotal / SellQuantity;

            var latest = Transactions.MaxBy(t => t.TransactionDate);
            Stock = latest.CurrentStock;
            StockValue = Stock * latest.PerpetualAverageCost;
            PerpetualAverageCost = latest.PerpetualAverageCost;

            PeriodicAverageProfit = SellTotal - AvgBuyPrice * SellQuantity;
            PerpetualAverageProfit = SellTotal - PerpetualAverageTotalCost;
            if (SellQuantity > 0)
                ProfitPerItem = PerpetualAverageProfit / SellQuantity;
            if (AvgSellPrice > 0)
                AvgMargin = (double)(ProfitPerItem / AvgSellPrice);

            if (groups.Any() && TradeDuration.TotalDays > 0)
                AvgProfitPerDay = PerpetualAverageProfit / (decimal)TradeDuration.TotalDays;
        }
    }
}