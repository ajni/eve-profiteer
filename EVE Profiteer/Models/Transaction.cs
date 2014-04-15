using System;
using System.ComponentModel.DataAnnotations;
using eZet.EveLib.Modules.Models;

namespace eZet.EveProfiteer.Models {

    public class Transaction {

        private decimal total;

        [Key]
        public int Id { get; set; }

        public DateTime TransactionDate { get; set; }

        public long TransactionId { get; set; }

        public int Quantity { get; set; }

        public string TypeName { get; set; }

        public long TypeId { get; set; }

        public decimal Price { get; set; }

        public decimal Total {
            get {
                if (total == 0) total = Quantity * Price;
                return total;
            }
            set { total = value; }
        }

        public long ClientId { get; set; }

        public string ClientName { get; set; }

        public long StationId { get; set; }

        public string StationName { get; set; }

        public OrderType TransactionType { get; set; }

        public string TransactionFor { get; set; }

        public virtual ApiKeyEntity ApiKeyEntity { get; set; }
    }
}
