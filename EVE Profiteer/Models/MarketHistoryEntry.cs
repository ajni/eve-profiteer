using System;
using eZet.EveLib.Modules.Models;

namespace eZet.EveProfiteer.Models {
    
    public class MarketHistoryEntry {

        public static MarketHistoryEntry Create(int typeId, int regionId, CrestMarketHistory.MarketHistoryEntry history) {
            var entry = new MarketHistoryEntry();
            entry.TypeId = typeId;
            entry.RegionId = regionId;
            entry.AvgPrice = history.AvgPrice;
            entry.Date = history.Date;
            entry.HighPrice = history.HighPrice;
            entry.LowPrice = history.LowPrice;
            entry.OrderCount = history.OrderCount;
            entry.Volume = history.Volume;
            return entry;
        }

        public static MarketHistoryEntry Create(EveData.MarketData.MarketHistoryEntry source) {
            var entry = new MarketHistoryEntry();
            entry.TypeId = source.TypeId;
            entry.RegionId = source.RegionId;
            entry.AvgPrice = source.AvgPrice;
            entry.Date = source.Date;
            entry.HighPrice = source.HighPrice;
            entry.LowPrice = source.LowPrice;
            entry.OrderCount = source.OrderCount;
            entry.Volume = source.Volume;
            return entry;
        }

        public static MarketHistoryEntry Create(EmdItemHistory.ItemHistoryEntry source) {
            var entry = new MarketHistoryEntry();
            entry.TypeId = source.TypeId;
            entry.RegionId = source.RegionId;
            entry.AvgPrice = source.AvgPrice;
            entry.Date = DateTime.Parse(source.Date);
            entry.HighPrice = source.MaxPrice;
            entry.LowPrice = source.MinPrice;
            entry.OrderCount = source.Orders;
            entry.Volume = source.Volume;
            return entry;
        }

        public long Id { get; set; }

        public int TypeId { get; set; }

        public int RegionId { get; set; }

        /// <summary>
        /// The volume of items moved
        /// </summary>
        public long Volume { get; set; }

        /// <summary>
        /// The number of orders
        /// </summary>
        public long OrderCount { get; set; }

        /// <summary>
        /// The lowest price
        /// </summary>
        public decimal LowPrice { get; set; }

        /// <summary>
        /// The highst price
        /// </summary>
        public decimal HighPrice { get; set; }

        /// <summary>
        /// The average price
        /// </summary>
        public decimal AvgPrice { get; set; }

        /// <summary>
        /// The date this entry represents
        /// </summary>
        public DateTime Date { get; set; }

     
    }
}
