using System.Linq;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Services {
    public class EveDataService {

        private EveDbContext db = new EveDbContext();

        public void SetLazyLoad(bool val) {
            db.Configuration.LazyLoadingEnabled = val;
        }

        public Item GetItem(long id) {
            var query = from item in db.Items
                        where item.TypeId == id
                        select item;
            return query.First();
        }

        public IQueryable<Item> GetItems() {
            var query = from item in db.Items
                        select item;
            return query;
        }

        public MarketGroup GetMarketGroup(long id) {
            var query = from g in db.MarketGroups
                        where g.MarketGroupId == id
                        select g;
            return query.First();

        }

        public IQueryable<MarketGroup> GetMarketGroups() {
            var query = from g in db.MarketGroups
                        select g;
            return query;
        }

        public IQueryable<Region> GetRegions() {
            var query = from row in db.Regions
                        where row.RegionId < 11000001
                        orderby row.RegionName
                        select row;
            return query;
        }
    }
}
