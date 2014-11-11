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
            using (EveProfiteerRepository db = CreateDb()) {
                List<MarketOrder> list =
                    await
                        db.MyMarketOrders()
                            .AsNoTracking()
                            .Include(e => e.Station)
                            .Include(e => e.InvType.Orders)
                            .ToListAsync()
                            .ConfigureAwait(false);
                list.Where(e => e.OrderState == OrderState.Open)
                    .Apply(
                        o =>
                            o.InvType.Orders =
                                o.InvType.Orders.Where(
                                    f =>
                                        f.ApiKeyEntity_Id == ApplicationHelper.ActiveEntity.Id && o.Bid
                                            ? f.IsBuyOrder
                                            : f.IsSellOrder).ToList());
                return list;
            }
        }
    }
}