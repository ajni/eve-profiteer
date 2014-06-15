using System.Linq;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;

namespace eZet.EveProfiteer.Services {
    public class EveProfiteerDataService {

        public EveProfiteerDataService() {
        }

        public EveProfiteerDataService(EveProfiteerDbEntities db) {
            Db = db;
        }

        public EveProfiteerDbEntities Db { get; private set; }

        public IQueryable<Order> GetOrders() {
            return Db.Orders.Where(order => order.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id);
        }

        public IQueryable<InvBlueprintType> GetBlueprints() {
            return Db.InvBlueprintTypes.Include("BlueprintInvType").Include("ProductInvType").Where(bp => bp.BlueprintInvType.Published == true);
        }

    }
}