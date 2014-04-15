using System;
using System.Runtime.Serialization;
using System.Xml;

namespace eZet.EvePilotOrderEditor.Models {

    public class BuyOrder {

        [DataMember(Name= "itemName")]
        public string ItemName { get; set; }

        [DataMember(Name = "itemID")]
        public long ItemId { get; set; }

        [DataMember(Name = "maxPrice")]
        public decimal MaxPrice { get; set; }

        [DataMember(Name = "quantity")]
        public int Quantity { get; set; }

        [DataMember(Name = "updateTime")]
        public DateTime UpdateTime { get; set; }

    }
}
