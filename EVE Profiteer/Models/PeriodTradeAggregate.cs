using System;
using System.Collections.Generic;
using System.Linq;

namespace eZet.EveProfiteer.Models {
    public class PeriodTradeAggregate {
        public ICollection<Transaction> Transactions { get; set; }

        public ICollection<ItemTradeAggregate> DailyTotals { get; private set; }

        public DateTime DateStart { get; private set; }

        public DateTime DateEnd { get; private set; }


        public PeriodTradeAggregate(ICollection<Transaction> transactions) {
            Transactions = transactions;
            DailyTotals = new List<ItemTradeAggregate>();
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
            foreach (var transactionCollection in dateGroups.ToList()) {
                DailyTotals.Add(new ItemTradeAggregate(transactionCollection.First().TransactionDate.Date, transactionCollection.First().InvType, transactionCollection));
            }
            foreach (var day in DailyTotals) {
                Incoming += day.Incoming;
                Outgoing += day.Outgoing;
                Profit += day.Profit;
                Balance += day.Balance;
            }
        }
    }
}
