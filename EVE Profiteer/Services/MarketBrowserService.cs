using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using eZet.EveLib.Modules.Models;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;
using OrderType = eZet.EveLib.Modules.OrderType;

namespace eZet.EveProfiteer.Services {
    public class MarketBrowserService : DbContextService {
        private readonly EveMarketService _eveMarketService;



        public MarketBrowserService(EveMarketService eveMarketService) {
            _eveMarketService = eveMarketService;
        }

        public async Task<MapRegion> GetDefaultRegion() {
            using (var db = CreateDb()) {
                return (await db.MapRegions.Where(region => region.RegionId == ConfigManager.DefaultRegion).ToListAsync().ConfigureAwait(false)).Single();
            }
        }

        public async Task<List<MapRegion>> GetRegions() {
            using (var db = CreateDb()) {
                return await db.MapRegions.AsNoTracking().ToListAsync().ConfigureAwait(false);
            }
        }

        public async Task<List<InvType>> GetMarketTypes() {
            using (var db = CreateDb()) {
                return await GetMarketTypes(db).AsNoTracking().ToListAsync().ConfigureAwait(false);
            }
        }

        public async Task<BindableCollection<TreeNode>> GetMarketTree() {
            using (var db = CreateDb()) {
                var rootList = new BindableCollection<TreeNode>();
                List<InvType> items = await GetMarketTypes();
                List<InvMarketGroup> groupList = await db.InvMarketGroups.AsNoTracking().ToListAsync();
                ILookup<int, TreeNode> groups = groupList.Select(t => new TreeNode(t)).ToLookup(t => t.Id);
                foreach (InvType item in items) {
                    var node = new TreeNode(item);
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

        public async Task<MarketBrowserItem> GetMarketDetails(MapRegion region, InvType invType) {
            EveMarketDataResponse<EmdItemOrders> orderResponse = await _eveMarketService.GetItemOrderAsync(region.RegionId, invType.TypeId).ConfigureAwait(false);
            var buyOrders = new List<MarketBrowserOrder>();
            var sellOrders = new List<MarketBrowserOrder>();
            foreach (EmdItemOrders.ItemOrderEntry order in orderResponse.Result.Orders) {
                MarketBrowserOrder marketBrowserOrder = ApiEntityMapper.Map(order, new MarketBrowserOrder());
                if (order.OrderType == OrderType.Buy) {
                    buyOrders.Add(marketBrowserOrder);
                } else {
                    sellOrders.Add(marketBrowserOrder);
                }
            }
            EveCrestMarketHistory history = await _eveMarketService.GetMarketHistoryAsync(region.RegionId, invType.TypeId).ConfigureAwait(false);
            var marketHistory = history.Entries.Select(entry => ApiEntityMapper.Map(entry, new MarketHistoryEntry())).ToList();

            var item = new MarketBrowserItem(invType, marketHistory, sellOrders, buyOrders, 7);
            return item;
        }
    }
}
