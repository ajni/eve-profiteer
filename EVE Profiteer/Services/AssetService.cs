using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using eZet.EveLib.Modules;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Services {
    public class AssetService : DbContextService {
        private readonly EveMarketService _marketService;
        private readonly EveProfiteerRepository _eveProfiteerRepository;

        private readonly TraceSource _trace = new TraceSource("EveProfiteer", SourceLevels.All);

        public AssetService(EveMarketService marketService, EveProfiteerRepository eveProfiteerRepository) {
            _marketService = marketService;
            _eveProfiteerRepository = eveProfiteerRepository;
        }

        public async Task<List<Asset>> GetAssets() {
            using (var db = CreateDb()) {
                return await MyAssets(db).Include(f => f.invType.MarketOrders).ToListAsync().ConfigureAwait(false);
            }
        }

        public async Task UpdateMarketData(IEnumerable<AssetViewModel> list, int region, int station, int days) {
            var items = list.Select(asset => asset.TypeId).ToList();
            var priceResult = await _marketService.GetItemPricesAsync(station, items).ConfigureAwait(false);
            var historyResult = await _marketService.GetItemHistoryAsync(region, items, days).ConfigureAwait(false);
            var priceLookup = priceResult.Prices.ToLookup(i => i.TypeId);
            var historyLookup = historyResult.ToLookup(i => i.TypeId);
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
