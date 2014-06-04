using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace eZet.EveProfiteer.Models {
    public class TradeAggregate {
        public TradeAggregate() {
            FirstTransactionDate = DateTime.MaxValue;
            LastTransactionDate = DateTime.MinValue;
            MinSellPrice = decimal.MaxValue;
            MinBuyPrice = decimal.MaxValue;
            TradeAggregates = new List<TradeAggregate>();
        }

        public TradeAggregate(IEnumerable<Transaction> transactions)
            : this() {
            Transactions = transactions;
            processTransactions();
            calculateTotals();
        }

        public TradeAggregate(InvType invType, IEnumerable<Transaction> transactions, Order order = null)
            : this() {
            InvType = invType;
            Transactions = transactions;
            Order = order;
            processTransactions();
            calculateTotals();
        }

        public TradeAggregate(ICollection<TradeAggregate> tradeAggregates)
            : this() {
            TradeAggregates = tradeAggregates;
            processAggregates();
            calculateTotals();
        }

        public TradeAggregate(IEnumerable<IGrouping<DateTime, Transaction>> grouping)
            : this() {
            foreach (var group in grouping) {
                var entry = new TradeAggregate(group);
                processAggregate(entry);
                TradeAggregates.Add(entry);
            }
            processAggregateCollection();
            calculateTotals();
        }

        public TradeAggregate(IEnumerable<IGrouping<DateTime, Transaction>> grouping, InvType invType, Order order)
            : this() {
            InvType = invType;
            Order = order;
            foreach (var group in grouping) {
                var entry = new TradeAggregate(group);
                processAggregate(entry);
                TradeAggregates.Add(entry);
            }
            processAggregateCollection();
            calculateTotals();
        }

        public TradeAggregate(IEnumerable<IGrouping<InvType, Transaction>> grouping)
            : this() {
            foreach (var group in grouping) {
                var entry = new TradeAggregate(group.Key, group);
                processAggregate(entry);
                TradeAggregates.Add(entry);
            }
            processAggregateCollection();
            calculateTotals();
        }

        public ICollection<TradeAggregate> TradeAggregates { get; private set; }

        public IEnumerable<Transaction> Transactions { get; private set; }

        public Order Order { get; private set; }

        public InvType InvType { get; private set; }


        public decimal Balance { get; private set; }


        public decimal PerpetualAverageGrossProfit { get; private set; }

        public decimal PerpetualAverageTotalCost { get; private set; }

        public decimal BuyTotal { get; private set; }

        public decimal SellTotal { get; private set; }

        public int BuyQuantity { get; private set; }

        public int SellQuantity { get; private set; }

        public decimal? MinBuyPrice { get; private set; }

        public decimal? AvgBuyPrice { get; private set; }

        public decimal? MaxBuyPrice { get; private set; }

        public decimal? MinSellPrice { get; private set; }

        public decimal? AvgSellPrice { get; private set; }

        public decimal? MaxSellPrice { get; private set; }

        public int Stock { get; private set; }

        public decimal StockValue { get; private set; }

        public int StockDelta { get; private set; }

        public int UnaccountedStock { get; private set; }

        public DateTime FirstTransactionDate { get; private set; }

        public DateTime LastTransactionDate { get; private set; }

        public TimeSpan TradeDuration { get; private set; }

        public decimal AvgProfitPerDay { get; private set; }

        public decimal AvgProfitPerUnit { get; private set; }

        public double AvgMargin { get; private set; }

        public decimal ClosingSellPrice { get; private set; }

        public decimal ClosingBuyPrice { get; private set; }

        public decimal OpeningBuyPrice { get; private set; }

        public decimal OpeningSellPrice { get; private set; }

        public decimal PerpetualAverageCost { get; private set; }


        private void calculateTotals() {
            TradeDuration = LastTransactionDate - FirstTransactionDate;

            if (BuyQuantity > 0)
                AvgBuyPrice = BuyTotal / BuyQuantity;
            if (SellQuantity > 0)
                AvgSellPrice = SellTotal / SellQuantity;

            Balance = SellTotal - BuyTotal;
            PerpetualAverageGrossProfit = SellTotal - PerpetualAverageTotalCost;

            StockDelta = BuyQuantity - SellQuantity;

            AvgProfitPerDay = PerpetualAverageGrossProfit;
            if (TradeDuration.TotalDays > 0)
                AvgProfitPerDay /= (int)(Math.Ceiling(TradeDuration.TotalDays));

            if (SellQuantity > 0)
                AvgProfitPerUnit = PerpetualAverageGrossProfit / SellQuantity;
            if (AvgSellPrice > 0)
                AvgMargin = (double)(AvgProfitPerUnit / AvgSellPrice);

            // Set prices to NULL if they are 0, as they have not been set.
            MinSellPrice = MinSellPrice == decimal.MaxValue ? null : MinSellPrice;
            MaxSellPrice = MaxSellPrice == default(decimal) ? null : MaxSellPrice;
            MaxBuyPrice = MaxBuyPrice == default(decimal) ? null : MaxBuyPrice;
            MinBuyPrice = MinBuyPrice == decimal.MaxValue ? null : MinBuyPrice;
        }

        private void processTransactions() {
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

            // TODO Only makes sense for type aggregates
            Transaction latest = Transactions.MaxBy(t => t.TransactionDate);
            Stock = latest.CurrentStock;
            StockValue = Stock * latest.PerpetualAverageCost;
            PerpetualAverageCost = latest.PerpetualAverageCost;


            List<Transaction> buys = Transactions.Where(t => t.TransactionType == TransactionType.Buy).ToList();
            if (buys.Any()) {
                Transaction firstBuy = buys.MinBy(t => t.TransactionDate);
                if (firstBuy != null)
                    OpeningBuyPrice = firstBuy.Price;
                Transaction lastBuy = buys.MaxBy(t => t.TransactionDate);
                if (lastBuy != null)
                    ClosingBuyPrice = lastBuy.Price;
            }

            List<Transaction> sells = Transactions.Where(t => t.TransactionType == TransactionType.Sell).ToList();
            if (sells.Any()) {
                Transaction firstSell = sells.MaxBy(t => t.TransactionDate);
                if (firstSell != null)
                    OpeningSellPrice = firstSell.Price;
                Transaction lastSell = sells.MaxBy(t => t.TransactionDate);
                if (lastSell != null)
                    ClosingSellPrice = lastSell.Price;
            }
        }

        private void processAggregates() {
            foreach (TradeAggregate entry in TradeAggregates) {
                processAggregate(entry);
            }
            processAggregateCollection();
        }

        private void processAggregateCollection() {
            if (!TradeAggregates.Any()) return;
            TradeAggregate latest = TradeAggregates.MaxBy(t => t.LastTransactionDate);
            Stock = latest.Stock;
            StockValue = latest.StockValue;

            TradeAggregate first = TradeAggregates.MinBy(t => t.FirstTransactionDate);
            if (first != null) {
                OpeningBuyPrice = first.OpeningBuyPrice;
                OpeningSellPrice = first.OpeningSellPrice;
            }

            TradeAggregate last = TradeAggregates.MaxBy(t => t.FirstTransactionDate);
            if (last != null) {
                ClosingBuyPrice = last.ClosingBuyPrice;
                ClosingSellPrice = last.ClosingSellPrice;
                PerpetualAverageCost = last.PerpetualAverageCost;
            }
        }

        private void processAggregate(TradeAggregate aggregate) {
            if (aggregate.FirstTransactionDate < FirstTransactionDate)
                FirstTransactionDate = aggregate.FirstTransactionDate;
            if (aggregate.LastTransactionDate > LastTransactionDate)
                LastTransactionDate = aggregate.LastTransactionDate;
            BuyTotal += aggregate.BuyTotal;
            SellTotal += aggregate.SellTotal;
            BuyQuantity += aggregate.BuyQuantity;
            SellQuantity += aggregate.SellQuantity;
            PerpetualAverageTotalCost += aggregate.PerpetualAverageTotalCost;
            UnaccountedStock += aggregate.UnaccountedStock;
        }
    }
}