using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace eZet.EveProfiteer.Models {
    public class TradeDetailsChartPoint {
        public DateTime Date { get; set; }
        public ICollection<Transaction> Transactions { get; set; }

        public TradeDetailsChartPoint(DateTime date, ICollection<Transaction> transactions) {
            Date = date;
            Transactions = transactions;
            initialize();
        }

        public decimal PeriodicAverageProfit { get; private set; }

        public decimal PerpetualAverageProfit { get; private set; }

        public decimal PerpetualAverageTotalCost { get; private set; }

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

        public int UnaccountedStock { get; private set; }

        public decimal StockValue { get; private set; }

        public decimal ClosingSellPrice { get; private set; }

        public decimal ClosingBuyPrice { get; private set; }


        private void initialize() {
            MinSellPrice = decimal.MaxValue;
            MinBuyPrice = decimal.MaxValue;
            foreach (var transaction in Transactions) {
                if (transaction.TransactionType == TransactionType.Sell) {
                    MinSellPrice = Math.Min(MinSellPrice.GetValueOrDefault(), transaction.Price);
                    MaxSellPrice = Math.Max(MaxSellPrice.GetValueOrDefault(), transaction.Price);
                    SellQuantity += transaction.Quantity;
                    SellTotal += transaction.Quantity * transaction.Price;
                    PerpetualAverageTotalCost += transaction.Quantity * transaction.PerpetualAverageCost;
                    UnaccountedStock += transaction.UnaccountedStock;
                } else if (transaction.TransactionType == TransactionType.Buy) {
                    MinBuyPrice = Math.Min(MinBuyPrice.GetValueOrDefault(), transaction.Price);
                    MaxBuyPrice = Math.Max(MaxBuyPrice.GetValueOrDefault(), transaction.Price);
                    BuyQuantity += transaction.Quantity;
                    BuyTotal += transaction.Quantity * transaction.Price;
                } else {
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
            StockDelta = BuyQuantity - SellQuantity;
            var latest = Transactions.MaxBy(t => t.TransactionDate);
            Stock = latest.CurrentStock;
            StockValue = Stock * latest.PerpetualAverageCost;

            var lastBuy = Transactions.LastOrDefault(t => t.TransactionType == TransactionType.Buy);
            if (lastBuy != null)
                ClosingBuyPrice = lastBuy.Price;
            var lastSell = Transactions.LastOrDefault(t => t.TransactionType == TransactionType.Sell);
            if (lastSell != null)
                ClosingSellPrice = lastSell.Price;


            MinSellPrice = MinSellPrice == decimal.MaxValue ? null : MinSellPrice;
            MaxSellPrice = MaxSellPrice == default(decimal) ? null : MaxSellPrice;
            MaxBuyPrice = MaxBuyPrice == default(decimal) ? null : MaxBuyPrice;
            MinBuyPrice = MinBuyPrice == decimal.MaxValue ? null : MinBuyPrice;

        }

    }
}
