using System.Linq;
using eZet.EveOnlineDbModels;

namespace eZet.EveProfiteer.Services {
    public class EveDataService {

        private EveDbContext db = new EveDbContext();

        public void SetLazyLoad(bool val) {
            db.Configuration.LazyLoadingEnabled = val;
        }

        public InvType GetType(long id) {
            var query = from item in db.InvTypes
                        where item.TypeId == id
                        select item;
            return query.Single();
        }

        public IQueryable<InvType> GetTypes() {
            var query = from item in db.InvTypes
                        select item;
            return query;
        }

        public InvMarketGroup GetMarketGroup(long id) {
            var query = from g in db.InvMarketGroups
                        where g.MarketGroupId == id
                        select g;
            return query.First();

        }

        public IQueryable<InvMarketGroup> GetMarketGroups() {
            var query = from g in db.InvMarketGroups
                        select g;
            return query;
        }

        public IQueryable<MapRegion> GetRegions() {
            var query = from row in db.MapRegions
                        where row.RegionId < 11000001
                        orderby row.RegionName
                        select row;
            return query;
        }
    }
}
