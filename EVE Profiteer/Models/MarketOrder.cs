using System;

namespace eZet.EveProfiteer.Models {
    public class MarketOrder {
        public int TypeId { get; set; }

        public int StationId { get; set; }

        public int SolarSystemId { get; set; }

        public int RegionId { get; set; }

        public decimal Price { get; set; }

        public long OrderId { get; set; }

        public int VolEntered { get; set; }

        public int VolRemaining { get; set; }

        public int MinVolume { get; set; }

        public int Range { get; set; }

        public DateTime IssuedDate { get; set; }

        public DateTime ExpiresDate { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}