using eZet.EveLib.Modules.Models.Character;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Util {
    public static class Mapper {
        public static Transaction Map(WalletTransactions.Transaction source, Transaction target) {
            target.TransactionId = source.TransactionId;
            target.TransactionDate = source.TransactionDate;
            target.Quantity = source.Quantity;
            target.TypeName = source.TypeName;
            target.TypeId = source.TypeId;target.Price = source.Price;
            target.ClientId = source.ClientId;
            target.ClientName = source.ClientName;
            target.StationId = source.StationId;
            target.StationName = source.StationName;
            target.TransactionType = source.TransactionType.ToString();
            target.TransactionFor = source.TransactionFor;
            target.JournalTransactionId = source.JournalTransactionId;
            target.ClientTypeId = source.ClientTypeId;
            return target;
        }

        public static JournalEntry Map(WalletJournal.JournalEntry source, JournalEntry target) {
            target.Date = source.Date;
            target.RefId = source.RefId;
            target.refTypeId = source.refTypeId;
            target.OwnerId = source.OwnerId;
            target.OwnerName = source.OwnerName;
            target.ParticipantId = source.ParticipantId;
            target.ParticipantName = source.ParticipantName;
            target.ArgumentId = source.ArgumentId;
            target.ArgumentName = source.ArgumentName;
            target.Amount = source.Amount;
            target.BalanceAfter = source.BalanceAfter;
            target.Reason = source.Reason;
            target.TaxReceiverId = source.TaxReceiverId;
            target.TaxAmount = source.TaxAmount;
            return target;
        }
    }
}