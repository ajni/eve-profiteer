using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;

namespace eZet.EveProfiteer.Services {

    public class EveProfiteerRepository {

        public const int DustTypeidLimit = 350000;

        public EveProfiteerDbEntities Db { get; private set; }

        public EveProfiteerRepository(EveProfiteerDbEntities db) {
            Db = db;
        }

        public Task SaveAsync() {
            return Db.SaveChangesAsync();
        }

        public Task ReloadAsync<T>(T entity) where T : class {
            return Db.Entry(entity).ReloadAsync();
        }

        public IOrderedQueryable<MapRegion> GetRegionsOrdered() {
            return Db.MapRegions.OrderBy(r => r.RegionName);
        }

        public IQueryable<InvType> GetInvTypes() {
            return Db.InvTypes.AsQueryable();
        }

        public IQueryable<InvType> GetMarketTypes() {
            return Db.InvTypes.Where(t => t.TypeId < DustTypeidLimit && t.Published == true && t.MarketGroupId != null).OrderBy(t => t.TypeName);
        }

        public IQueryable<InvMarketGroup> GetMarketGroups() {
            return Db.InvMarketGroups.AsQueryable();
        }

        public IQueryable<InvBlueprintType> GetBlueprints() {
            return Db.InvBlueprintTypes.Include(f => f.BlueprintInvType).Include(f => f.ProductInvType).Where(bp => bp.BlueprintInvType.Published == true);
        }

        public IQueryable<Transaction> MyTransactions() {
            return Db.Transactions.Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveEntity.Id);
        }

        public IQueryable<Order> MyOrders() {
            return Db.Orders.Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveEntity.Id);
        }

        public IQueryable<MarketOrder> MyMarketOrders() {
            return Db.MarketOrders.Where(t => t.ApiKeyEntityId == ApplicationHelper.ActiveEntity.Id);
        }
    }
}
