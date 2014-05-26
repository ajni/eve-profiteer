using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Services {
    public class ItemCostService {
        public EveProfiteerDbEntities Db { get; set; }

        public ItemCostService(EveProfiteerDbEntities db) {
            Db = db;
        }

        public async Task updateCosts(ApiKeyEntity entity) {
            var time = DateTime.UtcNow.AddMonths(-1).Date;
            Db.Configuration.AutoDetectChangesEnabled = false;
            var transactionsByType = await Db.Transactions.AsNoTracking()
                        .Where(t => t.ApiKeyEntity_Id == entity.Id && t.TransactionDate > time && t.TransactionType == TransactionType.Buy)
                        .GroupBy(t => t.TypeId).ToListAsync();
            foreach (var transactionList in transactionsByType) {
                var typeId = transactionList.Key;
                var cost = transactionList.Sum(t => t.Price)/transactionList.Count();
                var entry = Db.ItemCosts.SingleOrDefault(t => t.InvTypes_TypeId == typeId && t.ApiKeyEntities_Id == entity.Id);
                if (entry == null) {
                    entry = Db.ItemCosts.Create();
                    entry.ApiKeyEntities_Id = entity.Id;
                    entry.InvTypes_TypeId = typeId;
                }
                entry.MovingAverage = cost;
            }
            Db.ChangeTracker.DetectChanges();
            int n = await Db.SaveChangesAsync();
        }

        public IQueryable<ItemCost> GetItemCosts(ApiKeyEntity entity) {
            return Db.ItemCosts.AsNoTracking().Where(t => t.ApiKeyEntities_Id == entity.Id);
        }
    }
}