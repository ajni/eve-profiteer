using System.Collections.Generic;
using eZet.EveLib.Modules.Models;

namespace eZet.EveProfiteer.Models {
    public class MarketAnalyzer {

        public Dictionary<long, MarketAnalyzerResult> Results { get; set; }

        private readonly ICollection<Item> itemData;

        private readonly List<ItemHistory.ItemHistoryEntry> itemHistory;

        private Region region;

        public MarketAnalyzer(Region region, ICollection<Item> itemData) {
            this.region = region;
            this.itemData = itemData;
            this.itemHistory = new List<ItemHistory.ItemHistoryEntry>();
            Results = new Dictionary<long, MarketAnalyzerResult>();
        }

        public void Add(ICollection<ItemHistory.ItemHistoryEntry> history) {
            this.itemHistory.AddRange(history);
        }

        public void Calculate() {
            foreach (var item in itemData) {
                Results.Add(item.TypeId, new MarketAnalyzerResult(region, item));
            }
            foreach (var item in itemHistory) {
                Results[item.TypeId].Data.Add(item);
            }
            foreach (var item in Results.Values) {
                if (item.Data.Count != 0)
                    item.Calculate();
            }
        }

    }
}
