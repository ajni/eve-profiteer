using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Services {
    public class MarketOrderService : DbContextService {

        public async Task<List<MarketOrder>> GetMarketOrdersAsync() {
            using (var db = CreateDb()) {
                return await db.MarketOrders.AsNoTracking().Include("InvType").Include("Station").ToListAsync().ConfigureAwait(false);
            }
        }

    }
}
