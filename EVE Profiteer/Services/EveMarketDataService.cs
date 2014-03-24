using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using eZet.Eve.EveProfiteer.Entities;
using eZet.EveLib.EveMarketData;
using eZet.EveLib.EveMarketData.Model;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Services {

    public class EveMarketService {

        private readonly EveMarketData api = new EveMarketData(Format.Json);

        public async Task<MarketAnalyzer> GetMarketAnalyzer(Region region, ICollection<Item> items, int days, IProgress<ProgressType> progress) {
            progress.Report(new ProgressType(0, "Configuring query..."));
            var options = new EveMarketDataOptions();
            options.Regions.Add(region.RegionId);
            options.AgeSpan = TimeSpan.FromDays(days);
            var result = new MarketAnalyzer(region, items);
            var tasks = new List<Task<EveMarketDataResponse<ItemHistory>>>();
            foreach (var item in items) {
                options.Items.Add(item.TypeId);
                if (options.Items.Count > 10) {
                    progress.Report(new ProgressType(25, "Fetching history data..."));
                    tasks.Add(getItemHistory(options));
                    progress.Report(new ProgressType(50, "Initializing analysis..."));
                    options.Items.Clear();
                }
            }
            foreach (var res in await Task.WhenAll(tasks)) {
                result.Add(res.Result.History);
            }
            progress.Report(new ProgressType(75, "Analyzing..."));
            result.Calculate();
            progress.Report(new ProgressType(100, "Finished."));
            return result;
        }

        public Uri GetScannerLink(ICollection<Int64> items) {
            var options = new EveMarketDataOptions { Items = items };
            return api.GetScannerUri(options);
        }

        private async Task<EveMarketDataResponse<ItemHistory>> getItemHistory(EveMarketDataOptions options) {
            return await Task.Run(() => api.GetItemHistory(options));
        }
    }
}
