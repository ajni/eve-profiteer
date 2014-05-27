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
            initialize();
        }

        public ICollection<Transaction> Transactions { get; set; }
        public TradeInterval Interval { get; set; }

  


        public decimal SellTotal { get; private set; }

        public decimal BuyTotal { get; private set; }

        public decimal Balance { get; private set; }

        public decimal Profit { get; private set; }

        public decimal TransactionCount { get; private set; }


        public DateTime LastTransactionDate { get; set; }

        public DateTime FirstTransactionDate { get; set; }

        private void initialize() {
    
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