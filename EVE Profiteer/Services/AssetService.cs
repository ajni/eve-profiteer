using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using eZet.EveLib.Modules;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;

namespace eZet.EveProfiteer.Services {
    public class AssetService {
        private readonly EveProfiteerDataService _dataService;
        private readonly EveApiService _eveApiService;
        private readonly EveMarketService _marketService;

        private readonly TraceSource _trace = new TraceSource("EveProfiteer", SourceLevels.All);

        public AssetService(EveProfiteerDataService dataService, EveApiService eveApiService, EveMarketService marketService) {
            _dataService = dataService;
            _eveApiService = eveApiService;
            _marketService = marketService;
        }

        public async Task<int> UpdateAssets() {
            var result = await _eveApiService.GetAssetsAsync(ApplicationHelper.ActiveKey, ApplicationHelper.ActiveKeyEntity).ConfigureAwait(false);
            var groups = result.Flatten().Where(asset => !asset.Singleton).GroupBy(asset => asset.TypeId).ToList();
            var assets = await _dataService.GetAssets().ToListAsync().ConfigureAwait(false);
            var lookup = assets.ToLookup(asset => asset.InvTypes_TypeId);
            foreach (var group in groups) {
                var asset = lookup[group.Key].SingleOrDefault();
                if (asset == null) {
                    asset = new Asset();
                    asset.InvTypes_TypeId = group.Key;
                    asset.ApiKeyEntity_Id = ApplicationHelper.ActiveKeyEntity.Id;
                    _dataService.Db.Assets.Add(asset);
                }
                asset.ActualQuantity = group.Sum(item => item.Quantity);
                assets.Remove(asset);
            }
            foreach (var asset in assets) {
                asset.ActualQuantity = 0;
            }
            _trace.TraceEvent(TraceEventType.Verbose, 0, "Processed {0} assets.", groups.Count());
            return await _dataService.Db.SaveChangesAsync().ConfigureAwait(false);
        }

        public IQueryable<Asset> GetAssets() {
            return _dataService.Db.Assets.Where(asset => asset.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id);
        }

        public async Task UpdateAssetData(IEnumerable<AssetEntry> list, int region, int station, int days) {
            var items = list.Select(asset => asset.TypeId).ToList();
            var prices = await _marketService.GetPriceData(items, station).ConfigureAwait(false);
            var history = await _marketService.GetHistoryData(items, region, days).ConfigureAwait(false);
            var priceLookup = prices.ToLookup(i => i.TypeId);
            var historyLookup = history.ToLookup(i => i.TypeId);
            foreach (var asset in list) {
                var buyPrice =
                    priceLookup[asset.TypeId].Single(price => price.OrderType == OrderType.Buy).Price;
                var sellPrice =
                    priceLookup[asset.TypeId].Single(price => price.OrderType == OrderType.Sell).Price;
                asset.Update(historyLookup[asset.TypeId], sellPrice, buyPrice);
            }
        }
    }
}
