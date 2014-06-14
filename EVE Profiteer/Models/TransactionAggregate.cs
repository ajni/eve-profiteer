using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace eZet.EveProfiteer.Models {
    public class TransactionAggregate {
        public TransactionAggregate() {
            FirstTransactionDate = DateTime.MaxValue;
            LastTransactionDate = DateTime.MinValue;
            MinSellPrice = decimal.MaxValue;
            MinBuyPrice = decimal.MaxValue;
            TradeAggregates = new List<TransactionAggregate>();
        }

        public TransactionAggregate(IEnumerable<Transaction> transactions)
            : this() {
            Transactions = transactions;
            processTransactions();
            calculateTotals();
        }

        public TransactionAggregate(InvType invType, IEnumerable<Transaction> transactions, Order order = null)
            : this() {
            InvType = invType;
            Transactions = transactions;
            Order = order;
            processTransactions();
            calculateTotals();
        }

        public TransactionAggregate(ICollection<TransactionAggregate> tradeAggregates)
            : this() {
            TradeAggregates = tradeAggregates;
            processAggregates();
            calculateTotals();
        }

        public TransactionAggregate(IEnumerable<IGrouping<DateTime, Transaction>> grouping)
            : this() {
            foreach (var group in grouping) {
                var entry = new TransactionAggregate(group);
                processAggregate(entry);
                TradeAggregates.Add(entry);
            }
            processAggregateCollection();
            calculateTotals();
        }

        public TransactionAggregate(IEnumerable<IGrouping<DateTime, Transaction>> grouping, InvType invType, Order order)
            : this() {
            InvType = invType;
            Order = order;
            foreach (var group in grouping) {
                var entry = new TransactionAggregate(group);
                processAggregate(entry);
                TradeAggregates.Add(entry);
            }
            processAggregateCollection();
            calculateTotals();
        }

        public TransactionAggregate(IEnumerable<IGrouping<InvType, Transaction>> grouping)
            : this() {
            foreach (var group in grouping) {
                var entry = new TransactionAggregate(group.Key, group);
                processAggregate(entry);
                TradeAggregates.Add(entry);
            }
            processAggregateCollection();
            calculateTotals();
        }

        public ICollection<TransactionAggregate> TradeAggregates { get; private set; }

        public IEnumerable<Transaction> Transactions { get; private set; }

        public Order Order { get; private set; }

        public InvType InvType { get; private set; }

        #region Period

        public decimal Balance { get; private set; }

        public decimal MaterialCost { get; private set; }

        public decimal Sales { get; private set; }

        public decimal BuyOrderBrokerFees { get; private set; }

        public decimal SellOrderBrokerFees { get; private set; }

        public DateTime FirstTransactionDate { get; private set; }

        public DateTime LastTransactionDate { get; private set; }

        public TimeSpan TradeDuration { get; private set; }

        #endregion

        #region Profit

        public decimal GrossProfit { get; private set; }

        public decimal AvgGrossProfitPerDay { get; private set; }

        public decimal AvgGrossProfitPerUnit { get; private set; }

        public decimal OperatingProfit { get; private set; }

        public decimal AvgOperatingProfitPerDay { get; private set; }

        public decimal AvgOperatingProfitPerUnit { get; private set; }

        public decimal NetProfit { get; private set; }

        public decimal AvgNetProfitPerDay { get; private set; }

        public decimal AvgNetProfitPerUnit { get; private set; }


        #endregion

        #region Margins

        public double AvgGrossMargin { get; private set; }

        public double AvgOperatingMargin { get; private set; }

        public double AvgNetMargin { get; private set; }

        #endregion

        #region Expenses

        public decimal CostOfGoodsSold { get; private set; }

        public decimal MaterialCostOfGoodsSold { get; private set; }

        public decimal BrokerFeesOfGoodsSold { get; private set; }

        public decimal PerpetualAverageCost { get; private set; }

        public decimal SgaExpenses { get; private set; }

        public decimal SalesTax { get; private set; }

        #endregion

        #region Prices

        public decimal? MinBuyPrice { get; private set; }

        public decimal? AvgBuyPrice { get; private set; }

        public decimal? MaxBuyPrice { get; private set; }

        public decimal? MinSellPrice { get; private set; }

        public decimal? AvgSellPrice { get; private set; }

        public decimal? MaxSellPrice { get; private set; }

        public decimal ClosingSellPrice { get; private set; }

        public decimal ClosingBuyPrice { get; private set; }

        public decimal OpeningBuyPrice { get; private set; }

        public decimal OpeningSellPrice { get; private set; }

        #endregion

        #region Quantities

        public int BuyQuantity { get; private set; }

        public int SellQuantity { get; private set; }

        public int Stock { get; private set; }

        public decimal InventoryValue { get; private set; }

        public int StockDelta { get; private set; }

        public int UnaccountedStock { get; private set; }

        #endregion

        private void calculateTotals() {
            TradeDuration = LastTransactionDate - FirstTransactionDate;

            if (BuyQuantity > 0)
                AvgBuyPrice = MaterialCost / BuyQuantity;
            if (SellQuantity > 0)
                AvgSellPrice = Sales / SellQuantity;

            Balance = Sales - MaterialCost;

            SgaExpenses = SellOrderBrokerFees;

            StockDelta = BuyQuantity - SellQuantity;

            if (CostOfGoodsSold != 0) {
                GrossProfit = Sales - CostOfGoodsSold;
                OperatingProfit = GrossProfit - SgaExpenses;
                NetProfit = OperatingProfit - SalesTax;
            }

            AvgGrossProfitPerDay = GrossProfit;
            AvgOperatingProfitPerDay = OperatingProfit;
            AvgNetProfitPerDay = NetProfit;
            if (TradeDuration.TotalDays > 0) {
                AvgGrossProfitPerDay /= (int)(Math.Ceiling(TradeDuration.TotalDays));
                AvgOperatingProfitPerDay /= (int)(Math.Ceiling(TradeDuration.TotalDays));
                AvgNetProfitPerDay /= (int)(Math.Ceiling(TradeDuration.TotalDays));
            }

            if (SellQuantity > 0) {
                AvgGrossProfitPerUnit = GrossProfit / SellQuantity;
                AvgOperatingProfitPerUnit = OperatingProfit / SellQuantity;
                AvgNetProfitPerUnit = NetProfit / SellQuantity;
            }
            if (AvgSellPrice > 0) {
                AvgGrossMargin = (double)(AvgGrossProfitPerUnit / AvgSellPrice);
                AvgOperatingMargin = (double)(AvgOperatingProfitPerUnit / AvgSellPrice);
                AvgNetMargin = (double)(AvgNetProfitPerUnit / AvgSellPrice);
            }

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
                        MaterialCost += transaction.Quantity * transaction.Price;
                        BuyOrderBrokerFees += transaction.BrokerFee;
                        break;
                    case TransactionType.Sell:
                        MinSellPrice = Math.Min(MinSellPrice.GetValueOrDefault(), transaction.Price);
                        MaxSellPrice = Math.Max(MaxSellPrice.GetValueOrDefault(), transaction.Price);
                        SellQuantity += transaction.Quantity;
                        Sales += transaction.Quantity * transaction.Price;
                        CostOfGoodsSold += transaction.PerpetualAverageCost * transaction.Quantity;
                        UnaccountedStock += transaction.UnaccountedQuantity;
                        SalesTax += transaction.TaxLiability;
                        SellOrderBrokerFees += transaction.BrokerFee;
                        MaterialCostOfGoodsSold += transaction.CogsMaterialCost.GetValueOrDefault();
                        BrokerFeesOfGoodsSold += transaction.CogsBrokerFees.GetValueOrDefault();
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            // TODO Only makes sense for type aggregates
            Transaction latest = Transactions.MaxBy(t => t.TransactionDate);
            Stock = latest.PostTransactionStock;
            InventoryValue = Stock * latest.PerpetualAverageCost;
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
            foreach (TransactionAggregate entry in TradeAggregates) {
                processAggregate(entry);
            }
            processAggregateCollection();
        }

        private void processAggregateCollection() {
            if (!TradeAggregates.Any()) return;
            TransactionAggregate latest = TradeAggregates.MaxBy(t => t.LastTransactionDate);
            Stock = latest.Stock;
            InventoryValue = latest.InventoryValue;

            TransactionAggregate first = TradeAggregates.MinBy(t => t.FirstTransactionDate);
            if (first != null) {
                OpeningBuyPrice = first.OpeningBuyPrice;
                OpeningSellPrice = first.OpeningSellPrice;
            }

            TransactionAggregate last = TradeAggregates.MaxBy(t => t.FirstTransactionDate);
            if (last != null) {
                ClosingBuyPrice = last.ClosingBuyPrice;
                ClosingSellPrice = last.ClosingSellPrice;
                PerpetualAverageCost = last.PerpetualAverageCost;
            }
        }

        private void processAggregate(TransactionAggregate aggregate) {
            if (aggregate.FirstTransactionDate < FirstTransactionDate)
                FirstTransactionDate = aggregate.FirstTransactionDate;
            if (aggregate.LastTransactionDate > LastTransactionDate)
                LastTransactionDate = aggregate.LastTransactionDate;
            MaterialCost += aggregate.MaterialCost;
            Sales += aggregate.Sales;
            BuyQuantity += aggregate.BuyQuantity;
            SellQuantity += aggregate.SellQuantity;
            CostOfGoodsSold += aggregate.CostOfGoodsSold;
            UnaccountedStock += aggregate.UnaccountedStock;
            SalesTax += aggregate.SalesTax;
            BuyOrderBrokerFees += aggregate.BuyOrderBrokerFees;
            SellOrderBrokerFees += aggregate.SellOrderBrokerFees;
            MaterialCostOfGoodsSold += aggregate.MaterialCostOfGoodsSold;
            BrokerFeesOfGoodsSold += aggregate.BrokerFeesOfGoodsSold;
        }
    }
}