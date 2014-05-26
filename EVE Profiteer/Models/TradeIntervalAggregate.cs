using System;
using System.Collections.Generic;
using System.Linq;
using eZet.Utilities;

namespace eZet.EveProfiteer.Models {
    public enum TradeInterval {
        None,
        Hourly,
        Daily,
        Weekly,
        Monthly,
        Yearly
    }

    public class TradeIntervalAggregate {
        public TradeIntervalAggregate(IEnumerable<Transaction> transactions, TradeInterval interval = default(TradeInterval)) {
            Interval = interval;
            Transactions = transactions;
            Items = new List<TradeTypeAggregate>();
            initialize();
        }

        public TradeInterval Interval { get; set; }
        public IEnumerable<Transaction> Transactions { get; set; }

        public ICollection<TradeTypeAggregate> Items { get; private set; }

        public ICollection<TradeIntervalAggregate> Intervals { get; private set; }

        public decimal Incoming { get; private set; }

        public decimal Outgoing { get; private set; }

        public decimal Balance { get; private set; }

        public decimal Profit { get; private set; }

        public decimal TransactionCount { get; private set; }

        public DateTime LastTransactionDate { get; set; }

        public DateTime FirstTransactionDate { get; set; }

        public void CalculateTypeAggregates() {
            IEnumerable<IGrouping<int, Transaction>> groupByType = Transactions.GroupBy(t => t.TypeId);
            foreach (var transactionList in groupByType.ToList()) {
                Items.Add(new TradeTypeAggregate(transactionList.First().InvType, transactionList));
            }
            foreach (TradeTypeAggregate item in Items) {
                Incoming += item.SellTotal;
                Outgoing += item.BuyTotal;
                Profit += item.TotalProfit;
                Balance += item.Balance;
            }
        }

        public void CalculateSubIntervals() {
            IEnumerable<IGrouping<DateTime, Transaction>> groupByInterval =
     Transactions.GroupBy(t => toInterval(t.TransactionDate));
            foreach (var transactionList in groupByInterval.ToList()) {
                Intervals.Add(new TradeIntervalAggregate(transactionList));
            }
        }

        private void initialize() {
            FirstTransactionDate = DateTime.MaxValue;
            LastTransactionDate = DateTime.MinValue;
            foreach (Transaction transaction in Transactions) {
                if (transaction.TransactionDate < FirstTransactionDate)
                    FirstTransactionDate = transaction.TransactionDate;
                if (transaction.TransactionDate > LastTransactionDate)
                    LastTransactionDate = transaction.TransactionDate;
            }
            TransactionCount = Transactions.Count();
            if(Interval != TradeInterval.None)
                CalculateSubIntervals();

        }

        private DateTime toInterval(DateTime date) {
            switch (Interval) {
                case TradeInterval.Hourly:
                    return date.Date.AddHours(date.Hour);
                case TradeInterval.Daily:
                    return date.Date;
                case TradeInterval.Weekly:
                    return new DateTime(date.Year, 0, 0).AddDays(date.GetWeekOfYear()*7);
                case TradeInterval.Monthly:
                    return new DateTime(date.Year, date.Month, 0);
                case TradeInterval.Yearly:
                    return new DateTime(date.Year, 0, 0);
            }
            throw new InvalidOperationException();
        }
    }
}