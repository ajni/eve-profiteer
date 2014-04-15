using System.Collections.Generic;
using System.Runtime.Serialization;

namespace eZet.EvePilotOrderEditor.Models {
    [DataContract(Name = "DocumentElement")]
    public class BuyOrderCollection {

        [DataMember(Name = "BuyOrderInstallerList")]
        public List<BuyOrder> BuyOrders { get; set; }
    }
}
