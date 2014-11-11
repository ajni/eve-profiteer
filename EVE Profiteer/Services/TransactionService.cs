using System.Data.Entity;
using System.Linq;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Services {
    public class TransactionService : DbContextService {

        public IQueryable<Transaction> GetTransactions() {
            return Db.MyTransactions().Include("InvType");
        }

    }
}
