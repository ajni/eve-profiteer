using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;
namespace eZet.EveProfiteer.Services {
    public class MarketOrderService : DbContextService {

        public async Task<List<MarketOrder>> GetMarketOrdersAsync() {
            using (var db = CreateDb()) {
                var list = await db.MarketOrders.AsNoTracking().Include("Station").Include("InvType.Orders").ToListAsync().ConfigureAwait(false);
                list.Apply(o => o.InvType.Orders = o.InvType.Orders.Where(f => f.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id && f.IsBuyOrder || f.IsSellOrder).ToList());
                return list;
            }
        }
    }
}
