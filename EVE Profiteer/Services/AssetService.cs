using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using eZet.EveLib.Modules;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;

namespace eZet.EveProfiteer.Services {
    public class AssetService {
        private readonly EveProfiteerDataService _dataService;
        private readonly EveMarketService _marketService;

        private readonly TraceSource _trace = new TraceSource("EveProfiteer", SourceLevels.All);

        public AssetService(EveProfiteerDataService dataService, EveMarketService marketService) {
            _dataService = dataService;
            _marketService = marketService;
        }

        public IQueryable<Asset> GetAssets() {
            return _dataService.Db.Assets.Where(asset => asset.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id);
        }

        public async Task UpdateMarketData(IEnumerable<AssetEntry> list, int region, int station, int days) {
            var items = list.Select(asset => asset.TypeId).ToList();
            var priceResult = await _marketService.GetItemPricesAsync(station, items).ConfigureAwait(false);
            var historyResult = await _marketService.GetItemHistoryAsync(region, items, days).ConfigureAwait(false);
            var priceLookup = priceResult.Prices.ToLookup(i => i.TypeId);
            var historyLookup = historyResult.History.ToLookup(i => i.TypeId);
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
