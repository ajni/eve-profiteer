using eZet.EveLib.EveOnline.Model.Character;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Util {
    public static class Mapper {

        public static Transaction Map(WalletTransactions.Transaction source, Transaction target) {
            target.TransactionId = source.TransactionId;
            target.TransactionDate = source.TransactionDate;
            target.Quantity = source.Quantity;
            target.TypeName = source.TypeName;
            target.TypeId = source.TypeId;
            target.Price = source.Price;
            target.ClientId = source.ClientId;
            target.ClientName = source.ClientName;
            target.StationId = source.StationId;
            target.StationName = source.StationName;
            target.TransactionType = source.TransactionType;
            target.TransactionFor = source.TransactionFor;
            return target;
        }
    }
}
