using System;
using System.Collections.Generic;
using System.Linq;

namespace eZet.EveProfiteer.Models {
    public class TradeAnalyzerItem {
        public IEnumerable<Transaction> Transactions { get; set; }

        public TradeAnalyzerItem(int typeId, string typeName, IEnumerable<Transaction> transactions, Order order) {
            Transactions = transactions;
            TypeName = typeName;
            TypeId = typeId;
            Order = order;
            Analyze();
        }

        public TradeAnalyzerItem(IEnumerable<Transaction> transactions, Order order) {
            Order = order;
            Transactions = transactions;
            Analyze();
        }

        public void Analyze() {
            if (Transactions.Any()) {
                TypeId = Transactions.First().TypeId;
                TypeName = Transactions.First().InvType.TypeName;
            }
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
            Profit = AvgBuyPrice != 0 ? QuantitySold * AvgSellPrice - QuantitySold * AvgBuyPrice : 0;
            int span = (LastTransactionDate - FirstTransactionDate).Days;
            AvgProfitPerDay = Profit;
            if (span > 0)
                AvgProfitPerDay /= span;
        }

        public int TypeId { get; private set; }

        public string TypeName { get; private set; }

        public decimal Profit { get; private set; }

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

    }
}
