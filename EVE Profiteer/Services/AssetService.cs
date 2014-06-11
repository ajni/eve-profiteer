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
