using System;
using System.Collections.Generic;

namespace eZet.EveProfiteer.Models {
    public class TradeDetailsChartPoint {
        public DateTime Date { get; set; }
        public IEnumerable<Transaction> Transactions { get; set; }

        public TradeDetailsChartPoint(DateTime date, IEnumerable<Transaction> transactions, int stock) {
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

        public decimal? AvgBuyPrice { get; private set; }

        public decimal? AvgSellPrice { get; private set; }

        public decimal? MinBuyPrice { get; private set; }

        public decimal? MaxBuyPrice { get; private set; }

        public decimal? MinSellPrice { get; private set; }

        public decimal? MaxSellPrice { get; private set; }

        public int StockDelta { get; private set; }

        public int Stock { get; private set; }

        public decimal StockValue { get; private set; }


        private void initialize() {
            MinSellPrice = decimal.MaxValue;
            MinBuyPrice = decimal.MaxValue;
            foreach (var transaction in Transactions) {
                if (transaction.TransactionType == TransactionType.Sell) {
                    MinSellPrice = Math.Min(MinSellPrice.GetValueOrDefault(), transaction.Price);
                    MaxSellPrice = Math.Max(MaxSellPrice.GetValueOrDefault(), transaction.Price);
                    SellQuantity += transaction.Quantity;
                    SellTotal += transaction.Quantity * transaction.Price;
                } else if (transaction.TransactionType == TransactionType.Buy) {
                    MinBuyPrice = Math.Min(MinBuyPrice.GetValueOrDefault(), transaction.Price);
                    MaxBuyPrice = Math.Max(MaxBuyPrice.GetValueOrDefault(), transaction.Price);
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
            Profit = SellQuantity * AvgSellPrice.GetValueOrDefault() - SellQuantity * AvgBuyPrice.GetValueOrDefault();
            StockDelta = BuyQuantity - SellQuantity;
            Stock += StockDelta;
            StockValue = Stock * AvgBuyPrice.GetValueOrDefault();
            Stock = Stock < 0 ? 0 : Stock;

            MinSellPrice = MinSellPrice == decimal.MaxValue ? null : MinSellPrice;
            MaxSellPrice = MaxSellPrice == default(decimal) ? null : MaxSellPrice;
            MaxBuyPrice = MaxBuyPrice == default(decimal) ? null : MaxBuyPrice;
            MinBuyPrice = MinBuyPrice == decimal.MaxValue ? null : MinBuyPrice;

        }


    }
}
