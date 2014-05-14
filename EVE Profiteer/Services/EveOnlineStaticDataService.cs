using System.Linq;
using eZet.EveOnlineDbModels;

namespace eZet.EveProfiteer.Services {
    public class EveOnlineStaticDataService {
        public EveOnlineStaticDataService(EveDbContext repository) {
            Repository = repository;
        }

        private EveDbContext Repository { get; set; }

        public void SetLazyLoad(bool val) {
            Repository.Configuration.LazyLoadingEnabled = val;
        }

        public InvType GetType(long id) {
            IQueryable<InvType> query = from item in Repository.InvTypes
                where item.TypeId == id
                select item;
            return query.Single();
        }

        public IQueryable<InvType> GetTypes() {
            IQueryable<InvType> query = from item in Repository.InvTypes
                select item;
            return query;
        }

        public InvMarketGroup GetMarketGroup(long id) {
            IQueryable<InvMarketGroup> query = from g in Repository.InvMarketGroups
                where g.MarketGroupId == id
                select g;
            return query.First();
        }

        public IQueryable<InvMarketGroup> GetMarketGroups() {
            IQueryable<InvMarketGroup> query = from g in Repository.InvMarketGroups
                select g;
            return query;
        }

        public IQueryable<MapRegion> GetRegions() {
            IOrderedQueryable<MapRegion> query = from row in Repository.MapRegions
                where row.RegionId < 11000001
                orderby row.RegionName
                select row;
            return query;
        }
    }
}