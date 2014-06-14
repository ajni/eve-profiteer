using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace eZet.EveProfiteer.Models {
    public class TransactionAggregateSummary {
        public TransactionAggregateSummary(InvType invType, IEnumerable<Transaction> transactions, Order order) {
            InvType = invType;
            Transactions = transactions;
            Order = order;
            Entries = new List<TransactionAggregate>();
            initialize();
        }

        public TransactionAggregateSummary(IEnumerable<Transaction> transactions) {
            Transactions = transactions;
            Entries = new List<TransactionAggregate>();
            initialize();
        }

        public InvType InvType { get; set; }
        public IEnumerable<Transaction> Transactions { get; set; }
        public Order Order { get; set; }

        public ICollection<TransactionAggregate> Entries { get; set; }

        public decimal Balance { get; private set; }

        public decimal PeriodicAverageProfit { get; private set; }

        public decimal PerpetualAverageProfit { get; private set; }

        public decimal BuyTotal { get; private set; }

        public decimal SellTotal { get; private set; }

        public decimal AvgBuyPrice { get; private set; }

        public decimal AvgSellPrice { get; private set; }

        public decimal MinBuyPrice { get; private set; }

        public decimal MaxBuyPrice { get; private set; }

        public decimal MinSellPrice { get; private set; }

        public decimal MaxSellPrice { get; private set; }

        public int SellQuantity { get; private set; }

        public int BuyQuantity { get; private set; }

        public DateTime FirstTransactionDate { get; private set; }

        public DateTime LastTransactionDate { get; private set; }

        public decimal PerpetualAverageTotalCost { get; private set; }

        public decimal ProfitPerItem { get; private set; }

        public double AvgMargin { get; private set; }

        public int Stock { get; private set; }

        public int UnaccountedStock { get; private set; }

        public decimal StockValue { get; private set; }

        public decimal AvgProfitPerDay { get; private set; }

        public decimal PerpetualAverageCost { get; private set; }

        public TimeSpan TradeDuration { get; private set; }

        private void initialize() {
            List<List<Transaction>> groups =
                Transactions.GroupBy(f => f.TransactionDate.Date).Select(f => f.ToList()).ToList();
            FirstTransactionDate = DateTime.MaxValue;
            LastTransactionDate = DateTime.MinValue;
            foreach (var transactions in groups) {
                var entry = new TransactionAggregate(transactions);
                Entries.Add(entry);
                Balance += entry.Balance;
                SellQuantity += entry.SellQuantity;
                BuyQuantity += entry.BuyQuantity;
                BuyTotal += entry.MaterialCost;
                SellTotal += entry.Sales;
                PerpetualAverageTotalCost += entry.CostOfGoodsSold;
                UnaccountedStock += entry.UnaccountedStock;
                if (entry.FirstTransactionDate < FirstTransactionDate)
                    FirstTransactionDate = entry.FirstTransactionDate;
                if (entry.LastTransactionDate > LastTransactionDate)
                    LastTransactionDate = entry.LastTransactionDate;
            }

            TradeDuration = LastTransactionDate - FirstTransactionDate;

            if (BuyQuantity > 0)
                AvgBuyPrice = BuyTotal/BuyQuantity;
            if (SellQuantity > 0)
                AvgSellPrice = SellTotal/SellQuantity;

            Transaction latest = Transactions.MaxBy(t => t.TransactionDate);
            Stock = latest.PostTransactionStock;
            StockValue = Stock*latest.PerpetualAverageCost;
            PerpetualAverageCost = latest.PerpetualAverageCost;

            PeriodicAverageProfit = SellTotal - AvgBuyPrice*SellQuantity;
            PerpetualAverageProfit = SellTotal - PerpetualAverageTotalCost;
            if (SellQuantity > 0)
                ProfitPerItem = PerpetualAverageProfit/SellQuantity;
            if (AvgSellPrice > 0)
                AvgMargin = (double) (ProfitPerItem/AvgSellPrice);

            if (groups.Any() && TradeDuration.TotalDays > 0)
                AvgProfitPerDay = PerpetualAverageProfit/(decimal) TradeDuration.TotalDays;
        }
    }
}