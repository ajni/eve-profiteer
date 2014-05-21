using System.Collections.Generic;
using System.Linq;
using eZet.EveLib.Modules.Models;

namespace eZet.EveProfiteer.Models {
    public class MarketAnalyzer {
        public MarketAnalyzer(IEnumerable<InvType> itemData, IEnumerable<ItemPrices.ItemPriceEntry> sellOrders,
            IEnumerable<ItemPrices.ItemPriceEntry> buyOrders, IEnumerable<ItemHistory.ItemHistoryEntry> history) {
            this.itemData = itemData;
            this.sellOrders = sellOrders.ToDictionary(f => f.TypeId);
            this.buyOrders = buyOrders.ToDictionary(f => f.TypeId);
            this.history = history.ToLookup(f => f.TypeId);
            Result = new List<MarketAnalyzerEntry>();
        }

        public ICollection<MarketAnalyzerEntry> Result { get; set; }

        private IDictionary<long, ItemPrices.ItemPriceEntry> sellOrders { get; set; }

        private IDictionary<long, ItemPrices.ItemPriceEntry> buyOrders { get; set; }

        private ILookup<long, ItemHistory.ItemHistoryEntry> history { get; set; }

        private IEnumerable<InvType> itemData { get; set; }

        public void Analyze() {
            foreach (InvType item in itemData) {
                ItemPrices.ItemPriceEntry sellOrder, buyOrder;
                sellOrders.TryGetValue(item.TypeId, out sellOrder);
                buyOrders.TryGetValue(item.TypeId, out buyOrder);
                var itemHistory = new List<ItemHistory.ItemHistoryEntry>();
                if (history.Contains(item.TypeId)) {
                    itemHistory = history[item.TypeId].ToList();
                }
                Result.Add(new MarketAnalyzerEntry(item, sellOrder, buyOrder, itemHistory));
            }
        }
    }
}