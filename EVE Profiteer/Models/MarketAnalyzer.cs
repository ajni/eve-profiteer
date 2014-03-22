using System.Collections.Generic;
using System.Linq;
using eZet.EveLib.EveMarketData.Model;

namespace eZet.EveProfiteer.Models {
    public class MarketAnalyzer {

        public Dictionary<long, MarketAnalyzerResult> Items { get; set; }

        public MarketAnalyzer(Region region, IEnumerable<Item> items, EveMarketDataResponse<ItemHistory> history) {
            Items = new Dictionary<long, MarketAnalyzerResult>();
            var historyData = history.Result.History.ToList();
            //var currentData = current.Result.RowSet.Row.ToList();
            foreach (var item in items) {
                Items.Add(item.TypeId, new MarketAnalyzerResult(region, item));
            }
            foreach (var item in historyData) {
                Items[item.TypeId].Data.Add(item);
            }
        }

        public void Calculate() {
            foreach (var item in Items.Values) {
                if (item.Data.Count != 0)
                    item.Calculate();
            }
        }

    }
}
