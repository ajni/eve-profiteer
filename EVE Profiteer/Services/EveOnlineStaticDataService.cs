using System.Linq;
using eZet.EveOnlineDbModels;

namespace eZet.EveProfiteer.Services {
    public class EveOnlineStaticDataService {

        private EveDbContext Repository { get; set; }

        public EveOnlineStaticDataService(EveDbContext repository) {
            Repository = repository;
        }

        public void SetLazyLoad(bool val) {
            Repository.Configuration.LazyLoadingEnabled = val;
        }

        public InvType GetType(long id) {
            var query = from item in Repository.InvTypes
                        where item.TypeId == id
                        select item;
            return query.Single();
        }

        public IQueryable<InvType> GetTypes() {
            var query = from item in Repository.InvTypes
                        select item;
            return query;
        }

        public InvMarketGroup GetMarketGroup(long id) {
            var query = from g in Repository.InvMarketGroups
                        where g.MarketGroupId == id
                        select g;
            return query.First();

        }

        public IQueryable<InvMarketGroup> GetMarketGroups() {
            var query = from g in Repository.InvMarketGroups
                        select g;
            return query;
        }

        public IQueryable<MapRegion> GetRegions() {
            var query = from row in Repository.MapRegions
                        where row.RegionId < 11000001
                        orderby row.RegionName
                        select row;
            return query;
        }
    }
}
