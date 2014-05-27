using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace eZet.EveProfiteer.Models {
    public class TransactionAggregate {
        public TransactionAggregate(DateTime date, IEnumerable<Transaction> transactions) {
            Date = date;
            Transactions = transactions;
            initialize();
        }

        public TransactionAggregate(InvType invType, IEnumerable<Transaction> transactions, Order order = null) {
            InvType = invType;
            Transactions = transactions;
            Order = order;
            initialize();
        }

        public Order Order { get; private set; }

        public InvType InvType { get; private set; }

        public DateTime Date { get; private set; }

        public IEnumerable<Transaction> Transactions { get; private set; }

        public decimal PeriodicAverageProfit { get; private set; }

        public decimal PerpetualAverageProfit { get; private set; }

        public decimal PerpetualAverageTotalCost { get; private set; }

        public decimal Balance { get; private set; }

        public decimal BuyTotal { get; private set; }

        public decimal SellTotal { get; private set; }

        public int BuyQuantity { get; private set; }

        public int SellQuantity { get; private set; }

        public decimal? AvgBuyPrice { get; private set; }

        public decimal? AvgSellPrice { get; private set; }

        public decimal? MinBuyPrice { get; private set; }

        public decimal? MaxBuyPrice { get; private set; }

        public decimal? MinSellPrice { get; private set; }

        public decimal? MaxSellPrice { get; private set; }

        public int Stock { get; private set; }

        public decimal StockValue { get; private set; }

        public int StockDelta { get; private set; }

        public int UnaccountedStock { get; private set; }

        public DateTime FirstTransactionDate { get; private set; }

        public DateTime LastTransactionDate { get; private set; }

        public decimal AvgProfitPerDay { get; private set; }

        public decimal AvgProfitPerSale { get; private set; }

        public double AvgMargin { get; private set; }

        public decimal ClosingSellPrice { get; private set; }

        public decimal ClosingBuyPrice { get; private set; }

        private void initialize() {
            FirstTransactionDate = DateTime.MaxValue;
            LastTransactionDate = DateTime.MinValue;
            MinSellPrice = decimal.MaxValue;
            MinBuyPrice = decimal.MaxValue;
            foreach (Transaction transaction in Transactions) {
                // Set dates
                if (transaction.TransactionDate < FirstTransactionDate)
                    FirstTransactionDate = transaction.TransactionDate;
                if (transaction.TransactionDate > LastTransactionDate)
                    LastTransactionDate = transaction.TransactionDate;

                switch (transaction.TransactionType) {
                    case TransactionType.Buy:
                        MinBuyPrice = Math.Min(MinBuyPrice.GetValueOrDefault(), transaction.Price);
                        MaxBuyPrice = Math.Max(MaxBuyPrice.GetValueOrDefault(), transaction.Price);
                        BuyQuantity += transaction.Quantity;
                        BuyTotal += transaction.Quantity * transaction.Price;
                        break;
                    case TransactionType.Sell:
                        MinSellPrice = Math.Min(MinSellPrice.GetValueOrDefault(), transaction.Price);
                        MaxSellPrice = Math.Max(MaxSellPrice.GetValueOrDefault(), transaction.Price);
                        SellQuantity += transaction.Quantity;
                        SellTotal += transaction.Quantity * transaction.Price;
                        PerpetualAverageTotalCost += transaction.Quantity * transaction.PerpetualAverageCost;
                        UnaccountedStock += transaction.UnaccountedStock;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            if (BuyQuantity > 0)
                AvgBuyPrice = BuyTotal / BuyQuantity;
            if (SellQuantity > 0)
                AvgSellPrice = SellTotal / SellQuantity;

            Balance = SellTotal - BuyTotal;
            PeriodicAverageProfit = SellTotal - SellQuantity * AvgBuyPrice.GetValueOrDefault();
            PerpetualAverageProfit = SellTotal - PerpetualAverageTotalCost;

            Transaction latest = Transactions.MaxBy(t => t.TransactionDate);
            Stock = latest.CurrentStock;
            StockValue = Stock * latest.PerpetualAverageCost;
            StockDelta = BuyQuantity - SellQuantity;

            Transaction lastBuy = Transactions.LastOrDefault(t => t.TransactionType == TransactionType.Buy);
            if (lastBuy != null)
                ClosingBuyPrice = lastBuy.Price;
            Transaction lastSell = Transactions.LastOrDefault(t => t.TransactionType == TransactionType.Sell);
            if (lastSell != null)
                ClosingSellPrice = lastSell.Price;

            int span = (LastTransactionDate - FirstTransactionDate).Days;
            AvgProfitPerDay = PerpetualAverageProfit;
            if (span > 0)
                AvgProfitPerDay /= span;

            if (SellQuantity > 0)
                AvgProfitPerSale = PerpetualAverageProfit / SellQuantity;
            if (AvgSellPrice > 0)
                AvgMargin = (double)(AvgProfitPerSale / AvgSellPrice);


            MinSellPrice = MinSellPrice == decimal.MaxValue ? null : MinSellPrice;
            MaxSellPrice = MaxSellPrice == default(decimal) ? null : MaxSellPrice;
            MaxBuyPrice = MaxBuyPrice == default(decimal) ? null : MaxBuyPrice;
            MinBuyPrice = MinBuyPrice == decimal.MaxValue ? null : MinBuyPrice;
        }
    }
}