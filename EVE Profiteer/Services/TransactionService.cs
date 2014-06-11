using System.Data.Entity;
using System.Linq;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;

namespace eZet.EveProfiteer.Services {
    public class TransactionService {

        private EveProfiteerDbEntities _db;

        public IQueryable<Transaction> GetTransactions() {
            if (_db == null)
                _db = IoC.Get<EveProfiteerDbEntities>();
            return
                _db.Transactions.Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id).Include("InvType");
        }

        public void Deactivate() {
            if (_db != null) {
                _db.Dispose();
                _db = null;
            }
        }

    }
}
