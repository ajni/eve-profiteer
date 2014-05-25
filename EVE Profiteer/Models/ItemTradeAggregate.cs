using System;
using System.Collections.Generic;
using System.Linq;

namespace eZet.EveProfiteer.Models {
    public class ItemTradeAggregate {
        public DateTime Date { get; set; }
        public InvType Invtype { get; set; }
        public IEnumerable<Transaction> Transactions { get; set; }

        public ItemTradeAggregate(DateTime date, InvType invtype, IEnumerable<Transaction> transactions) {
            Date = date;
            Invtype = invtype;
            Transactions = transactions;
            Items = new List<TradeAnalyzerEntry>();
            initialize();
        }

        public ICollection<TradeAnalyzerEntry> Items { get; private set; }

        public decimal Incoming { get; private set; }

        public decimal Outgoing { get; private set; }

        public decimal Balance { get; private set; }

        public decimal Profit { get; private set; }

        public decimal TransactionCount { get; private set; }

        private void initialize() {
            TransactionCount = Transactions.Count();
            var itemGroups = Transactions.GroupBy(t => t.TypeId);
            foreach (var transactionCollection in itemGroups.ToList()) {
                Items.Add(new TradeAnalyzerEntry(transactionCollection.First().InvType, transactionCollection));
            }
            foreach (var item in Items) {
                Incoming += item.SellTotal;
                Outgoing += item.BuyTotal;
                Profit += item.TotalProfit;
                Balance += item.Balance;
            }
        }
    }
}
