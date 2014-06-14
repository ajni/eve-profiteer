using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;

namespace eZet.EveProfiteer.Services {
    public class TransactionDetailsService {
        private EveProfiteerDbEntities getDb() {
            return IoC.Get<EveProfiteerDbEntities>();
        }


        public async Task<List<InvType>> GetSelectableItems() {
            using (EveProfiteerDbEntities db = getDb()) {
                IQueryable<IGrouping<InvType, Transaction>> groups =
                    db.Transactions.AsNoTracking().Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id)
                        .GroupBy(t => t.InvType);
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
            using (EveProfiteerDbEntities db = getDb()) {
                return await
                    db.Transactions.Include("InvType").Where(f => f.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id &&
                                    f.TypeId == type.TypeId && f.TransactionDate >= start.Date &&
                                    f.TransactionDate <= end.Date).ToListAsync().ConfigureAwait(false);
            }
        }

        public async Task<Order> GetOrder(InvType type) {
            using (var db = getDb()) {
                type = db.InvTypes.Attach(type);
                var order = await db.Orders.Where(t => t.TypeId == type.TypeId && t.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id).ToListAsync().ConfigureAwait(false);
                return order.SingleOrDefault();
            }
        }
    }
}