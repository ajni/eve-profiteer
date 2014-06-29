using System.Linq;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;

namespace eZet.EveProfiteer.Services {
    public abstract class DbContextService {

        protected EveProfiteerDbEntities CreateDb() {
            return IoC.Get<EveProfiteerDbEntities>();
        }

        protected IQueryable<Transaction> MyTransactions(EveProfiteerDbEntities db) {
            return db.Transactions.Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id);
        }

        protected IQueryable<Order> MyOrders(EveProfiteerDbEntities db) {
            return db.Orders.Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id);
        }

        protected IQueryable<Asset> MyAssets(EveProfiteerDbEntities db) {
            return db.Assets.Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id);
        }

        protected IQueryable<MarketOrder> MyMarketOrders(EveProfiteerDbEntities db) {
            return db.MarketOrders.Where(t => t.ApiKeyEntityId == ApplicationHelper.ActiveKeyEntity.Id);
        }

        protected IQueryable<InvType> GetMarketTypes(EveProfiteerDbEntities db) {
            return db.InvTypes.Where(t => t.Published == true && t.MarketGroupId != null);
        }
    }
}