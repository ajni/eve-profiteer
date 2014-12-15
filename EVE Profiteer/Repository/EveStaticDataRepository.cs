using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;


namespace eZet.EveProfiteer.Repository {
    public class EveStaticDataRepository {

        public const int DustTypeidLimit = 350000;

        public EveProfiteerDbEntities Context { get; private set; }

        public readonly Task Initialize;


        public EveStaticDataRepository(EveProfiteerDbEntities context) {
            Context = context;
            Context.Configuration.AutoDetectChangesEnabled = false;
            Context.Configuration.LazyLoadingEnabled = false;
            Context.Configuration.ProxyCreationEnabled = false;
            Initialize = InitializeAsync();
        }

        public async Task InitializeAsync() {
            await Context.StaStations.LoadAsync().ConfigureAwait(false);
            await Context.MapRegions.LoadAsync().ConfigureAwait(false);
            await Context.InvTypes.LoadAsync().ConfigureAwait(false);
            await Context.InvMarketGroups.LoadAsync().ConfigureAwait(false);
        }

        public IQueryable<StaStation> GetStations() {
            return Context.StaStations.AsQueryable();
        }

        public IQueryable<MapRegion> GetRegions() {
            return Context.MapRegions.AsQueryable();
        }

        public IOrderedQueryable<MapRegion> GetRegionsOrdered() {
            return Context.MapRegions.OrderBy(r => r.RegionName);
        }

        public IQueryable<InvType> GetMarketTypes() {
            return Context.InvTypes.Where(t => t.TypeId < DustTypeidLimit && t.Published == true && t.MarketGroupId != null).OrderBy(t => t.TypeName);
        }

        public IQueryable<InvMarketGroup> GetMarketGroups() {
            return Context.InvMarketGroups.AsQueryable();
        }


        public async Task<BindableCollection<MarketTreeNode>> GetMarketTree() {
            var rootList = new BindableCollection<MarketTreeNode>();
            List<InvType> items = await GetMarketTypes().ToListAsync().ConfigureAwait(false);
            List<InvMarketGroup> groupList = await GetMarketGroups().ToListAsync().ConfigureAwait(false);
            ILookup<int, MarketTreeNode> groups = groupList.Select(t => new MarketTreeNode(t)).ToLookup(t => t.Id);
            foreach (InvType item in items) {
                var node = new MarketTreeNode(item);
                int id = item.MarketGroupId.GetValueOrDefault();
                var group = groups[id].Single();
                if (group != null) {
                    group.Children.Add(node);
                    node.Parent = group;
                }
            }

            foreach (var key in groupList) {
                var node = groups[key.MarketGroupId].Single();
                if (key.ParentGroupId.HasValue) {
                    var parent = groups[key.ParentGroupId.GetValueOrDefault()].Single();
                    parent.Children.Add(node);
                    node.Parent = parent;
                } else {
                    rootList.Add(node);
                }
            }
            return rootList;
        }

    }
}
