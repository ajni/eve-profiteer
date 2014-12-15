using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using eZet.EveLib.Modules.Models;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;
using OrderType = eZet.EveLib.Modules.OrderType;

namespace eZet.EveProfiteer.Services {
    public class AssetManagerService : DbContextService {
        private readonly EveMarketService _marketService;

        private readonly TraceSource _trace = new TraceSource("EveProfiteer", SourceLevels.All);

        public AssetManagerService(EveMarketService marketService) {
            _marketService = marketService;
        }

        public async Task<IEnumerable<AssetVm>> GetAssets() {
            List<Asset> assets = await
                Db.MyAssets()
                    .Include(a => a.invType.Orders)
                    .Include(a => a.AssetModifications)
                    .Include(a => a.staStation)
                    .ToListAsync()
                    .ConfigureAwait(false);

            return
                assets.Select(
                    asset =>
                        new AssetVm(asset,
                            asset.invType.Orders.Where(f => f.ApiKeyEntity_Id == ApplicationHelper.ActiveEntity.Id), asset.AssetModifications));
        }

        public async Task Save(IEnumerable<AssetVm> assets) {
                await Db.SaveChangesAsync().ConfigureAwait(false);
        }

        public void AddModification(AssetModification modification) {
            Db.Context.AssetModifications.Add(modification);
        }

        public async Task UpdateMarketData(IEnumerable<AssetVm> list, int region, int station, int days) {
            List<int> items = list.Select(asset => asset.TypeId).ToList();
            EmdItemPrices priceResult =
                await _marketService.GetItemPricesAsync(region, station, items).ConfigureAwait(false);
            IEnumerable<MarketHistoryEntry> historyResult =
                await _marketService.GetItemHistoryAsync(region, items, days).ConfigureAwait(false);
            ILookup<int, EmdItemPrices.ItemPriceEntry> priceLookup = priceResult.Prices.ToLookup(i => i.TypeId);
            ILookup<int, MarketHistoryEntry> historyLookup = historyResult.ToLookup(i => i.TypeId);
            foreach (AssetVm asset in list) {
                decimal buyPrice =
                    priceLookup[asset.TypeId].Single(price => price.OrderType == OrderType.Buy).Price;
                decimal sellPrice =
                    priceLookup[asset.TypeId].Single(price => price.OrderType == OrderType.Sell).Price;
                asset.Update(historyLookup[asset.TypeId], sellPrice, buyPrice);
            }
        }
    }
}