using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Services {
    public class TradeAnalyzerService : CurrentEntityService {

        public async Task<List<Transaction>> GetTransactions(DateTime start, DateTime end) {
            using (var db = CreateDb()) {
                return
                    await MyTransactions(db).Include("InvType").Where(t => t.TransactionDate >= start.Date && t.TransactionDate <= end.Date).ToListAsync().ConfigureAwait(false);
            }
        }

        public async Task<List<Order>> GetOrders() {
            using (var db = CreateDb()) {
                return await MyOrders(db).ToListAsync().ConfigureAwait(false);
            }
        }
    }
}
