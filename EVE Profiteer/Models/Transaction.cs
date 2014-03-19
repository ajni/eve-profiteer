using System;
using System.ComponentModel.DataAnnotations;

namespace eZet.EveProfiteer.Models {
    public class Transaction {

        [Key]
        public int Id { get; set; }

        public DateTime TransactionDate { get; set; }

        public long TransactionId { get; set; }

        public int Quantity { get; set; }

        public string TypeName { get; set; }

        public long TypeId { get; set; }

        public decimal Price { get; set; }

        public long ClientId { get; set; }

        public string ClientName { get; set; }

        public long StationId { get; set; }

        public string StationName { get; set; }

        public string TransactionType { get; set; }

        public string TransactionFor { get; set; }

        public virtual ApiKeyEntity ApiKeyEntity { get; set; }
    }
}
