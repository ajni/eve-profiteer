using System.Collections.Generic;
using System.Linq;

namespace eZet.EveProfiteer.Models {
    public class ItemDetailsData {
        public ItemDetailsData(int typeId, string typeName, IEnumerable<Transaction> transactions) {
            TypeId = typeId;
            TypeName = typeName;
            Transactions = transactions;
            ChartEntries = new List<ItemDetailsChartEntry>();
        }

        public IEnumerable<Transaction> Transactions { get; set; }

        public ICollection<ItemDetailsChartEntry> ChartEntries { get; set; }

        public int TypeId { get; private set; }

        public string TypeName { get; private set; }


        public void Analyze() {
            var group = Transactions.GroupBy(f => f.TransactionDate.Date);
            foreach (var transactionList in group.Select(f => f.ToList()).ToList()) {
                ChartEntries.Add(new ItemDetailsChartEntry(transactionList.First().TransactionDate.Date, transactionList));
            }
        }
    }
}