using System;
using System.Collections.Generic;
using System.Linq;

namespace eZet.EveProfiteer.Models {
    public class JournalAggregate<T> {

        public ICollection<JournalAggregate<T>> JournalAggregates { get; set; }

        public JournalAggregate(T key, IEnumerable<JournalEntry> journalEntries) {
            Key = key;
            processEntries(journalEntries);
        }

        public JournalAggregate(T key, IEnumerable<JournalAggregate<T>> journalAggregates)  {
            Key = key;
            processAggregates(journalAggregates);
        }

        public JournalAggregate(IEnumerable<IGrouping<T, JournalEntry>> journalGroups) {
            JournalAggregates = new List<JournalAggregate<T>>();
            foreach (var group in journalGroups) {
                JournalAggregates.Add(new JournalAggregate<T>(group.Key, group.ToList()));
            }
            processAggregates(JournalAggregates);
        }

        public T Key { get; private set; }

        public decimal CashFlow { get; private set; }

        public decimal Income { get; private set; }

        public decimal Outgo { get; private set; }

        public string TaxAmount { get; private set; }

        public decimal BalanceAfter { get; private set; }

        private void processEntries(IEnumerable<JournalEntry> journalEntries) {
            foreach (var entry in journalEntries.OrderBy(t => t.Date)) {
                CashFlow += entry.Amount;
                if (entry.Amount < 0) Outgo -= entry.Amount;
                else Income += entry.Amount;
                TaxAmount += entry.TaxAmount;
                BalanceAfter = entry.BalanceAfter;
            }
        }

        private void processAggregates(IEnumerable<JournalAggregate<T>> journalAggregates) {
            foreach (var aggregate in journalAggregates.OrderBy(t => Key)) {
                CashFlow += aggregate.CashFlow;
                TaxAmount += aggregate.TaxAmount;
                Income += aggregate.Income;
                Outgo += aggregate.Outgo;
                BalanceAfter = aggregate.BalanceAfter;
            }
        }



    }
}
