using System;
using System.Collections.Generic;

namespace eZet.EveProfiteer.Models {
    public class ItemDetailsChartEntry {
        public DateTime Date { get; set; }
        public IEnumerable<Transaction> Transactions { get; set; }

        public ItemDetailsChartEntry(DateTime date, IEnumerable<Transaction> transactions, int stock) {
            Date = date;
            Transactions = transactions;
            Stock = stock;
            initialize();
        }

        public decimal Profit { get; private set; }

        public decimal Balance { get; private set; }

        public decimal SellTotal { get; private set; }

        public int SellQuantity { get; private set; }

        public decimal BuyTotal { get; private set; }

        public int BuyQuantity { get; private set; }

        public decimal AvgBuyPrice { get; private set; }

        public decimal AvgSellPrice { get; private set; }

        public int StockDelta { get; private set; }

        public int Stock { get; private set; }

        public decimal StockValue { get; private set; }


        private void initialize() {
            foreach (var transaction in Transactions) {
                if (transaction.TransactionType == "Sell") {
                    SellQuantity += transaction.Quantity;
                    SellTotal += transaction.Quantity * transaction.Price;
                } else if (transaction.TransactionType == "Buy") {
                    BuyQuantity += transaction.Quantity;
                    BuyTotal += transaction.Quantity * transaction.Price;
                } else {
                    throw new NotImplementedException();
                }
            }
            if (BuyQuantity > 0)
                AvgBuyPrice = BuyTotal / BuyQuantity;
            if (SellQuantity > 0)
                AvgSellPrice = SellTotal / SellQuantity;
            Balance = SellTotal - BuyTotal;
            Profit = SellQuantity * AvgSellPrice - SellQuantity * AvgBuyPrice;
            StockDelta = BuyQuantity - SellQuantity;
            Stock += StockDelta;
            Stock = Stock < 0 ? 0 : Stock;
            StockValue = Stock * AvgBuyPrice;

        }


    }
}
