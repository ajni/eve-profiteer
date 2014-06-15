using System.Linq;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;

namespace eZet.EveProfiteer.Services {
    public abstract class CurrentEntityService {

         protected EveProfiteerDbEntities GetDb() {
            return IoC.Get<EveProfiteerDbEntities>();
        }

        public IQueryable<Transaction> MyTransactions(EveProfiteerDbEntities db) {
            return db.Transactions.Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id);
        }

        public IQueryable<Order> MyOrders(EveProfiteerDbEntities db) {
            return db.Orders.Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id);
        }


    }
}
