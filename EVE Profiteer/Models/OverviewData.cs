using System;
using System.Collections.Generic;
using System.Linq;

namespace eZet.EveProfiteer.Models {
    public class OverviewData {
        public ICollection<Transaction> Transactions { get; set; }

        public ICollection<TradeIntervalAggregate> Periods { get; private set; }

        public DateTime DateStart { get; private set; }

        public DateTime DateEnd { get; private set; }


        public OverviewData(ICollection<Transaction> transactions, DateTime dateStart, DateTime dateEnd) {
            DateEnd = dateEnd;
            DateStart = dateStart;
            Transactions = transactions;
            Periods = new List<TradeIntervalAggregate>();
            initialize();
        }

        public decimal Incoming { get; private set; }

        public decimal Outgoing { get; private set; }

        public decimal Balance { get; private set; }

        public decimal Profit { get; private set; }

        public decimal TransactionCount { get; private set; }

        private void initialize() {
            TransactionCount = Transactions.Count;
            var dateGroups = Transactions.GroupBy(t => t.TransactionDate.Date);
            var itemGroups = Transactions.GroupBy(t => t.TypeId);
            foreach (var transactionCollection in dateGroups.ToList()) {
                Periods.Add(new TradeIntervalAggregate(transactionCollection, TradeInterval.Daily));
            }
            foreach (var day in Periods) {
                Incoming += day.Incoming;
                Outgoing += day.Outgoing;
                Profit += day.Profit;
                Balance += day.Balance;
            }
        }
    }
}
