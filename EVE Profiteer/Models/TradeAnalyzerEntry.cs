using System;
using System.Collections.Generic;

namespace eZet.EveProfiteer.Models {
    public class TradeAnalyzerEntry {
        public IEnumerable<Transaction> Transactions { get; set; }

        public TradeAnalyzerEntry(InvType invType, IEnumerable<Transaction> transactions, Order order = null) {
            Order = order;
            InvType = invType;
            Transactions = transactions;
            initialize();
        }

        private void initialize() {
            // TODO Add LIFO or some other cost price calculation
            FirstTransactionDate = DateTime.MaxValue;
            LastTransactionDate = DateTime.MinValue;
            foreach (var transaction in Transactions) {
                if (transaction.TransactionDate < FirstTransactionDate)
                    FirstTransactionDate = transaction.TransactionDate;
                if (transaction.TransactionDate > LastTransactionDate)
                    LastTransactionDate = transaction.TransactionDate;

                if (transaction.TransactionType == "Sell") {
                    QuantitySold += transaction.Quantity;
                    SellTotal += transaction.Price * transaction.Quantity;
                } else if (transaction.TransactionType == "Buy") {
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
            TotalProfit = AvgBuyPrice != 0 ? QuantitySold * AvgSellPrice - QuantitySold * AvgBuyPrice : 0;
            int span = (LastTransactionDate - FirstTransactionDate).Days;
            AvgProfitPerDay = TotalProfit;
            if (span > 0)
                AvgProfitPerDay /= span;
            if (AvgSellPrice != 0 && AvgBuyPrice != 0) {
                AvgProfitPerSale = AvgSellPrice - AvgBuyPrice;
                AvgMargin = (double) (AvgProfitPerSale / AvgSellPrice);
            }
        }

        public InvType InvType { get; private set; }

        public decimal TotalProfit { get; private set; }

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

    }
}
