using System;
using System.Collections.Generic;
using System.Linq;

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

        public decimal Profit { get; private set; }

        public decimal ProfitPerItem { get; private set; }

        public double Margin { get; private set; }

        public decimal Balance { get; private set; }

        public int Stock { get; private set; }

        public decimal StockValue { get; private set; }

        public decimal AvgProfitPerDay { get; private set; }

        public TimeSpan TradeDuration { get; private set; }



        private void initialize() {
            var groups = Transactions.GroupBy(f => f.TransactionDate.Date).Select(f => f.ToList()).ToList();
            var sortedGroups = groups.OrderBy(f => f.First().TransactionDate.Date);
            FirstTrade = DateTime.MaxValue;
            LastTrade = DateTime.MinValue;
            var stock = 0;
            foreach (var transactions in sortedGroups) {
                var entry = new TradeDetailsChartPoint(transactions.First().TransactionDate.Date, transactions, stock);
                stock = entry.Stock;
                ChartEntries.Add(entry);
                Balance += entry.Balance;
                SellQuantity += entry.SellQuantity;
                BuyQuantity += entry.BuyQuantity;
                BuyTotal += entry.BuyTotal;
                SellTotal += entry.SellTotal;
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

            Stock = BuyQuantity - SellQuantity;
            StockValue = Stock * AvgBuyPrice;
            Profit = SellTotal - AvgBuyPrice * SellQuantity;
            ProfitPerItem = AvgSellPrice - AvgBuyPrice;
            Margin = (double) (ProfitPerItem / AvgSellPrice);

            if (groups.Any() && TradeDuration.TotalDays > 0)
                AvgProfitPerDay = Profit / (decimal)TradeDuration.TotalDays;
        }
    }
}