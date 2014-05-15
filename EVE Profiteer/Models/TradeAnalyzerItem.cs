using System;
using System.Collections.Generic;

namespace eZet.EveProfiteer.Models {
    public class TradeAnalyzerItem {
        public IEnumerable<Transaction> Transactions { get; set; }

        public TradeAnalyzerItem(long typeId, string typeName, IEnumerable<Transaction> transactions) {
            Transactions = transactions;
            TypeName = typeName;
            TypeId = typeId;
            analyze();
        }

        private void analyze() {
            foreach (var transaction in Transactions) {
                if (transaction.TransactionType == "Sell") {
                    QuantitySold += transaction.Quantity;
                    In += transaction.Price * transaction.Quantity;
                } else if (transaction.TransactionType == "Buy") {
                    QuantityBought += transaction.Quantity;
                    Out += transaction.Price * transaction.Quantity;
                }
            }
            Balance = In - Out;
            StockDelta = QuantityBought - QuantitySold;
            if (QuantityBought > 0)
                AvgBuyPrice = Out / QuantityBought;
            if (QuantitySold > 0)
                AvgSellPrice = In / QuantitySold;
            int units = QuantitySold < QuantityBought ? QuantitySold : QuantityBought;
            Profit = units * AvgSellPrice - units * AvgBuyPrice;
        }

        public long TypeId { get; private set; }

        public string TypeName { get; private set; }

        public decimal Profit { get; private set; }

        public decimal Balance { get; private set; }

        public decimal StockDelta { get; private set; }

        public decimal Out { get; private set; }

        public decimal In { get; private set; }

        public int QuantitySold { get; private set; }

        public int QuantityBought { get; private set; }


        public decimal AvgBuyPrice { get; private set; }

        public decimal AvgSellPrice { get; private set; }



    }
}
