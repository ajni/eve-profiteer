using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using eZet.EveLib.Modules;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;

namespace eZet.EveProfiteer.Services {
    public class MarketAnalyzerService : DbContextService {
        private readonly EveMarketService _eveMarketService;
        private readonly Repository _repository;

        public MarketAnalyzerService(EveMarketService eveMarketService, Repository repository) {
            _eveMarketService = eveMarketService;
            _repository = repository;
        }

        public async Task<BindableCollection<MarketTreeNode>> GetMarketTreeAsync(
            PropertyChangedEventHandler itemPropertyChanged) {
            var rootList = new BindableCollection<MarketTreeNode>();
            List<InvType> items = await _repository.GetMarketTypes().Include("Orders").ToListAsync().ConfigureAwait(false);
            List<InvMarketGroup> groupList = await _repository.GetMarketGroups().ToListAsync().ConfigureAwait(false);
            ILookup<int, MarketTreeNode> groups = groupList.Select(t => new MarketTreeNode(t)).ToLookup(t => t.Id);

            foreach (InvType item in items) {
                var node = new MarketTreeNode(item);
                int id = item.MarketGroupId.GetValueOrDefault();
                var group = groups[id].Single();
                if (group != null) {
                    group.Children.Add(node);
                    node.Parent = group;
                }
                node.PropertyChanged += itemPropertyChanged;
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

        public Task<List<MapRegion>> GetRegionsAsync() {
            return _repository.GetRegionsOrdered().Include("StaStations").ToListAsync();
        }

        public async Task<List<InvType>> GetInvTypesForOrdersAsync() {
            using (var db = CreateDb()) {
                return await MyOrders(db).Select(order => order.InvType).ToListAsync().ConfigureAwait(false);
            }
        }

        public async Task<IList<MarketAnalyzerEntry>> AnalyzeAsync(MapRegion region, StaStation station, IEnumerable<InvType> invTypes, int days) {
            var items = invTypes.Select(type => type.TypeId).ToList();
            var priceResult = await _eveMarketService.GetItemPricesAsync(station.StationId, items).ConfigureAwait(false);
            var historyResult = await _eveMarketService.GetItemHistoryAsync(region.RegionId, items, days).ConfigureAwait(false);
            var analyzer = new MarketAnalyzer(invTypes, priceResult.Prices.Where(o => o.OrderType == OrderType.Sell),
                priceResult.Prices.Where(o => o.OrderType == OrderType.Buy), historyResult.History);
            analyzer.Analyze();
            foreach (var entry in analyzer.Result) {
                entry.Order = entry.InvType.Orders.SingleOrDefault(order => order.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id);
            }
            return analyzer.Result;
        }
    }
}
