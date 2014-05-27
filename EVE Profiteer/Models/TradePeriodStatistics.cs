using System;
using System.Collections.Generic;
using System.Linq;

namespace eZet.EveProfiteer.Models {


    public class TradePeriodStatistics {
        public TradePeriodStatistics(DateTime date, Dictionary<InvType, decimal> prices, IEnumerable<Transaction> transactions, TradeInterval interval) {
            Date = date;
            Prices = prices;
            Interval = interval;
            Transactions = transactions;
            initialize();
        }

        public DateTime Date { get; set; }
        
        public Dictionary<InvType, decimal> Prices { get; set; }
        
        public TradeInterval Interval { get; set; }

        public IEnumerable<Transaction> Transactions { get; set; }

        public decimal Incoming { get; private set; }

        public decimal Outgoing { get; private set; }

        public decimal Balance { get; private set; }

        public decimal Profit { get; private set; }

        public decimal TransactionCount { get; private set; }

        private void initialize() {

            
        }


    }
}