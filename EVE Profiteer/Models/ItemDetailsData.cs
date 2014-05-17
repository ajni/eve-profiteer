using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace eZet.EveProfiteer.Models {
    public class ItemDetailsData {
        public ItemDetailsData(int typeId, string typeName, IEnumerable<Transaction> transactions) {
            TypeId = typeId;
            TypeName = typeName;
            Transactions = transactions;
            ChartEntries = new List<ItemDetailsChartEntry>();
            initialize();
        }

        public IEnumerable<Transaction> Transactions { get; set; }

        public ICollection<ItemDetailsChartEntry> ChartEntries { get; set; }

        public int TypeId { get; private set; }

        public string TypeName { get; private set; }

        public int SellQuantity { get; private set; }

        public int BuyQuantity { get; private set; }

        public DateTime FirstTrade { get; private set; }

        public DateTime LastTrade { get; private set; }

        public decimal BuyTotal { get; private set; }

        public decimal SellTotal { get; private set; }

        public decimal AvgBuyPrice { get; private set; }

        public decimal AvgSellPrice { get; private set; }

        public decimal Profit { get; private set; }

        public decimal Balance { get; private set; }

        public int Stock { get; private set; }

        public decimal StockValue { get; private set; }

        public decimal AvgProfitPerDay { get; private set; }

        public TimeSpan TradeDuration { get; private set; }



        private void initialize() {
            var groups = Transactions.GroupBy(f => f.TransactionDate.Date).Select(f => f.ToList()).ToList();
            var sortedGroups = groups.OrderBy(f => f.First().TransactionDate.Date);
            FirstTrade = DateTime.MaxValue;
            LastTrade = DateTime.MinValue;
            var stock = 0;
            foreach (var transactions in sortedGroups) {
                var entry = new ItemDetailsChartEntry(transactions.First().TransactionDate.Date, transactions, stock);
                stock = entry.Stock;
                ChartEntries.Add(entry);
                Balance += entry.Balance;
                SellQuantity += entry.SellQuantity;
                BuyQuantity += entry.BuyQuantity;
                BuyTotal += entry.BuyTotal;
                SellTotal += entry.SellTotal;
                if (entry.Date < FirstTrade)
                    FirstTrade = entry.Date;
                if (entry.Date > LastTrade)
                    LastTrade = entry.Date;
            }

            TradeDuration = LastTrade - FirstTrade;

            if (BuyQuantity > 0)
                AvgBuyPrice = BuyTotal / BuyQuantity;
            if (SellQuantity > 0)
                AvgSellPrice = SellTotal / SellQuantity;

            Stock = BuyQuantity - SellQuantity;
            StockValue = Stock * AvgBuyPrice;
            Profit = SellTotal - AvgBuyPrice * SellQuantity;

            if (groups.Any())
                AvgProfitPerDay = Profit / groups.Count();
        }
    }
}