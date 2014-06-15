using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;

namespace eZet.EveProfiteer.Services {
    public class TransactionDetailsService : DbContextService {
        public async Task<List<InvType>> GetSelectableItems() {
            using (EveProfiteerDbEntities db = CreateDb()) {
                IQueryable<IGrouping<InvType, Transaction>> groups =
                    MyTransactions(db).Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id)
                        .AsNoTracking().GroupBy(t => t.InvType);
                return
                    await
                        groups.AsNoTracking()
                            .Select(g => g.Key)
                            .OrderBy(t => t.TypeName)
                            .ToListAsync()
                            .ConfigureAwait(false);
            }
        }

        public async Task<List<Transaction>> GetTransactions(InvType type, DateTime start, DateTime end) {
            using (EveProfiteerDbEntities db = CreateDb()) {
                return await
                    MyTransactions(db).Where(f => f.TypeId == type.TypeId && f.TransactionDate >= start.Date &&
                                                  f.TransactionDate <= end.Date)
                        .Include("InvType")
                        .AsNoTracking()
                        .ToListAsync()
                        .ConfigureAwait(false);
            }
        }

        public async Task<Order> GetOrder(InvType type) {
            using (EveProfiteerDbEntities db = CreateDb()) {
                List<Order> order =
                    await MyOrders(db).Where(t => t.TypeId == type.TypeId).ToListAsync().ConfigureAwait(false);
                return order.SingleOrDefault();
            }
        }
    }
}