using System;
using System.Runtime.Serialization;

namespace eZet.Eve.OrderIoHelper.Models {

    [DataContract(Name="BuyOrderInstallerList", Namespace = "")]
    public class BuyOrder {

        [DataMember(Name = "itemName", IsRequired = true, Order = 0)]
        public string ItemName { get; set; }

        [DataMember(Name = "itemID", IsRequired = true, Order = 1)]
        public int ItemId { get; set; }

        [DataMember(Name = "maxPrice", IsRequired = true, Order = 2)]
        public long MaxPrice { get; set; }

        [DataMember(Name = "quantity", IsRequired = true, Order = 3)]
        public int Quantity { get; set; }

        [DataMember(Name = "updateTime", Order = 4)]
        public DateTime UpdateTime { get; set; }

    }
}
