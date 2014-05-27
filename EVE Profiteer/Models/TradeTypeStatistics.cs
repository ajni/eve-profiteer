using System;
using System.Collections.Generic;

namespace eZet.EveProfiteer.Models {
    public class TradeTypeStatistics {
        public IEnumerable<Transaction> Transactions { get; set; }

        public ICollection<TradePeriodStatistics> PeriodAggregates { get; private set; }

        public TradeTypeStatistics(InvType invType, IEnumerable<Transaction> transactions, Order order = null) {
            Order = order;
            InvType = invType;
            Transactions = transactions;
            initialize();
        }

        public void CalculatePeriodAggregate(TimeSpan timespan) {
            
        }

        private void initialize() {
            FirstTransactionDate = DateTime.MaxValue;
            LastTransactionDate = DateTime.MinValue;
            foreach (var transaction in Transactions) {
                if (transaction.TransactionDate < FirstTransactionDate)
                    FirstTransactionDate = transaction.TransactionDate;
                if (transaction.TransactionDate > LastTransactionDate)
                    LastTransactionDate = transaction.TransactionDate;

                if (transaction.TransactionType == TransactionType.Sell) {
                    QuantitySold += transaction.Quantity;
                    SellTotal += transaction.Price * transaction.Quantity;
                    CostOfGoodsSold += transaction.PerpetualAverageCost * transaction.Quantity;
                } else if (transaction.TransactionType == TransactionType.Buy) {
                    QuantityBought += transaction.Quantity;
                    BuyTotal += transaction.Price * transaction.Quantity;
                }
            }
            Balance = SellTotal - BuyTotal;
            StockDelta = QuantityBought - QuantitySold;
            if (QuantityBought > 0)
                AvgBuyPrice = BuyTotal / QuantityBought;
            if (QuantitySold > 0)
                AvgSellPrice = SellTotal / QuantitySold;
            PeriodicAverageProfit = AvgBuyPrice != 0 ? SellTotal - QuantitySold * AvgBuyPrice : 0;
            PerpetualAverageProfit = SellTotal - CostOfGoodsSold;
            int span = (LastTransactionDate - FirstTransactionDate).Days;
            AvgProfitPerDay = PeriodicAverageProfit;
            if (span > 0)
                AvgProfitPerDay /= span;
            if (AvgSellPrice != 0 && AvgBuyPrice != 0) {
                AvgProfitPerSale = AvgSellPrice - AvgBuyPrice;
                AvgMargin = (double) (AvgProfitPerSale / AvgSellPrice);
            }
        }

        public InvType InvType { get; private set; }

        public decimal PeriodicAverageProfit { get; private set; }

        public decimal Balance { get; private set; }

        public decimal StockDelta { get; private set; }

        public decimal BuyTotal { get; private set; }

        public decimal SellTotal { get; private set; }

        public int QuantitySold { get; private set; }

        public int QuantityBought { get; private set; }

        public decimal AvgBuyPrice { get; private set; }

        public decimal AvgSellPrice { get; private set; }

        public Order Order { get; set; }

        public DateTime FirstTransactionDate { get; private set; }

        public DateTime LastTransactionDate { get; private set; }

        public decimal AvgProfitPerDay { get; private set; }

        public decimal AvgProfitPerSale { get; private set; }

        public double AvgMargin { get; private set; }

        public decimal CostOfGoodsSold { get; private set; }

        public decimal PerpetualAverageProfit { get; private set; }


    }
}
