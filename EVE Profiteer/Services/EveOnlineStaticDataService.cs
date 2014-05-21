using System.Linq;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Services {
    public class EveOnlineStaticDataService {
        public EveOnlineStaticDataService(EveProfiteerDbEntities repository) {
            Repository = repository;
        }

        private EveProfiteerDbEntities Repository { get; set; }

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

        public IQueryable<mapRegion> GetRegions() {
            IOrderedQueryable<mapRegion> query = from row in Repository.mapRegions
                where row.RegionId < 11000001
                orderby row.RegionName
                select row;
            return query;
        }
    }
}