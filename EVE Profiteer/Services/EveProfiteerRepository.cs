using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;

namespace eZet.EveProfiteer.Services {

    public class EveProfiteerRepository : IDisposable {

        public const int DustTypeidLimit = 350000;

        public EveProfiteerDbEntities Context { get; private set; }

        public EveProfiteerRepository(EveProfiteerDbEntities context) {
            Context = context;
        }

        public Task SaveChangesAsync() {
            return Context.SaveChangesAsync();
        }

        public Task ReloadAsync<T>(T entity) where T : class {
            return Context.Entry(entity).ReloadAsync();
        }

        public IQueryable<Asset> MyAssets() {
            return Context.Assets.Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveEntity.Id);
        }

        public IOrderedQueryable<MapRegion> GetRegionsOrdered() {
            return Context.MapRegions.OrderBy(r => r.RegionName);
        }

        public IQueryable<InvType> GetInvTypes() {
            return Context.InvTypes.Where(e => e.TypeId < DustTypeidLimit);
        }

        public IQueryable<InvType> GetMarketTypes() {
            return Context.InvTypes.Where(t => t.TypeId < DustTypeidLimit && t.Published == true && t.MarketGroupId != null).OrderBy(t => t.TypeName);
        }

        public IQueryable<InvMarketGroup> GetMarketGroups() {
            return Context.InvMarketGroups.AsQueryable();
        }

        public IQueryable<InvBlueprintType> GetBlueprints() {
            return Context.InvBlueprintTypes.Include(f => f.BlueprintInvType).Include(f => f.ProductInvType).Where(bp => bp.BlueprintInvType.Published == true);
        }

        public IQueryable<Transaction> MyTransactions() {
            return Context.Transactions.Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveEntity.Id);
        }

        public IQueryable<Order> MyOrders() {
            return Context.Orders.Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveEntity.Id);
        }

        public IQueryable<MarketOrder> MyMarketOrders() {
            return Context.MarketOrders.Where(t => t.ApiKeyEntityId == ApplicationHelper.ActiveEntity.EntityId);
        }

        public void Dispose() {
            ((IDisposable) Context).Dispose();
        }

        public IQueryable<JournalEntry> MyJournalEntries() {
            return Context.JournalEntries.Include(f => f.RefType).Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveEntity.Id);
        }
    }
}
