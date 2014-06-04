using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace eZet.Eve.OrderXmlHelper.Models {
    [CollectionDataContract(Name = "DocumentElement", Namespace = "")]
    public class BuyOrderCollection : Collection<BuyOrder> {



    }
}
