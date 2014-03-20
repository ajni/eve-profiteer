using System;
using System.ComponentModel.DataAnnotations;


namespace eZet.EveProfiteer.Models {
    public class JournalEntry {

        [Key]
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public long RefId { get; set; }

        public long refTypeId { get; set; }

        public string OwnerName { get; set; }

        public long OwnerId { get; set; }

        public string ParticipantName { get; set; }

        public long ParticipantId { get; set; }

        public string ArgumentName { get; set; }

        public long ArgumentId { get; set; }

        public decimal Amount { get; set; }

        public decimal BalanceAfter { get; set; }

        public string Reason { get; set; }

        public string TaxReceiverId { get; set; }

        public string TaxAmount { get; set; }

        public virtual ApiKeyEntity ApiKeyEntity { get; set; }
    }
}
