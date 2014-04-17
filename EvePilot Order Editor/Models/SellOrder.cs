using System;
using System.Runtime.Serialization;

namespace eZet.Eve.OrderIoHelper.Models {

    [DataContract(Name = "SellOrderInstallerList", Namespace = "")]
    public class SellOrder {

        [DataMember(Name = "itemName", IsRequired = true, Order = 0)]
        public string ItemName { get; set; }

        [DataMember(Name = "itemID", IsRequired = true, Order = 1)]
        public long ItemId { get; set; }

        [DataMember(Name = "minPrice", IsRequired = true, Order = 2)]
        public decimal MinPrice { get; set; }

        [DataMember(Name = "quantity", IsRequired = true, Order = 3)]
        public int Quantity { get; set; }

        [DataMember(Name = "maxQuantity", IsRequired = true, Order = 4)]
        public int MaxQuantity { get; set; }

        [DataMember(Name = "updateTime", Order = 5)]
        public DateTime UpdateTime { get; set; }


    }
}
