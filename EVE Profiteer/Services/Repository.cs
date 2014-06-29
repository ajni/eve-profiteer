using System.Data.Entity;
using System.Linq;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;

namespace eZet.EveProfiteer.Services {

    public class Repository {

        public EveProfiteerDbEntities Db { get; set; }

        public Repository(EveProfiteerDbEntities db) {
            Db = db;
        }

        public IOrderedQueryable<MapRegion> GetRegionsOrdered() {
            return Db.MapRegions.OrderBy(r => r.RegionName);
        }

        public IQueryable<InvType> GetInvTypes() {
            return Db.InvTypes.AsQueryable();
        }

        public IQueryable<InvType> GetMarketTypes() {
            return Db.InvTypes.Where(t => t.Published == true && t.MarketGroupId != null).OrderBy(t => t.TypeName);
        }

        public IQueryable<InvMarketGroup> GetMarketGroups() {
            return Db.InvMarketGroups.AsQueryable();
        }

        public IQueryable<InvBlueprintType> GetBlueprints() {
            return Db.InvBlueprintTypes.Include(f => f.BlueprintInvType).Include(f => f.ProductInvType).Where(bp => bp.BlueprintInvType.Published == true);
        }

        public IQueryable<Transaction> MyTransactions() {
            return Db.Transactions.Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id);
        }

        public IQueryable<Order> MyOrders() {
            return Db.Orders.Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id);
        }

        public IQueryable<MarketOrder> MyMarketOrders() {
            return Db.MarketOrders.Where(t => t.ApiKeyEntityId == ApplicationHelper.ActiveKeyEntity.Id);
        }
    }
}
