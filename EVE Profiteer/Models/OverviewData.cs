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

    public class OverviewData {
        public OverviewData(ICollection<Transaction> transactions, TradeInterval interval) {
            Transactions = transactions;
            Interval = interval;
            Types = new List<TradeTypeStatistics>();
            initialize();
        }

        public ICollection<Transaction> Transactions { get; set; }
        public TradeInterval Interval { get; set; }

        public TradePeriodStatistics Period { get; private set; }

        public ICollection<TradeTypeStatistics> Types { get; private set; }

        public ICollection<TradePeriodStatistics> Periods { get; private set; }


        public decimal SellTotal { get; private set; }

        public decimal BuyTotal { get; private set; }

        public decimal Balance { get; private set; }

        public decimal Profit { get; private set; }

        public decimal TransactionCount { get; private set; }


        public DateTime LastTransactionDate { get; set; }

        public DateTime FirstTransactionDate { get; set; }

        private void initialize() {
            FirstTransactionDate = DateTime.MaxValue;
            LastTransactionDate = DateTime.MinValue;
            foreach (Transaction transaction in Transactions) {
                if (transaction.TransactionDate < FirstTransactionDate)
                    FirstTransactionDate = transaction.TransactionDate;
                if (transaction.TransactionDate > LastTransactionDate)
                    LastTransactionDate = transaction.TransactionDate;
            }
            TransactionCount = Transactions.Count;

            IEnumerable<IGrouping<int, Transaction>> itemGroups = Transactions.GroupBy(t => t.TypeId);
            foreach (var itemGroup in itemGroups) {
                Types.Add(new TradeTypeStatistics(itemGroup.First().InvType, (itemGroup)));
            }
            ILookup<int, TradeTypeStatistics> typeLookup = Types.ToLookup(t => t.InvType.TypeId);

            IEnumerable<IGrouping<DateTime, Transaction>> groupByInterval =
                Transactions.GroupBy(t => toInterval(t.TransactionDate));
            foreach (var transactionList in groupByInterval.ToList()) {
                //Periods.Add(new TradePeriodStatistics(Types.ToDictionary(f => f.InvType, f => f.AvgBuyPrice), transactionList));
            }
        }

        public void CalculateSubIntervals() {
        }

        private DateTime toInterval(DateTime date) {
            switch (Interval) {
                case TradeInterval.Hourly:
                    return date.Date.AddHours(date.Hour);
                case TradeInterval.Daily:
                    return date.Date;
                case TradeInterval.Weekly:
                    return new DateTime(date.Year, 0, 0).AddDays(date.GetWeekOfYear() * 7);
                case TradeInterval.Monthly:
                    return new DateTime(date.Year, date.Month, 0);
                case TradeInterval.Yearly:
                    return new DateTime(date.Year, 0, 0);
            }
            throw new InvalidOperationException();
        }
    }
}