﻿using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace eZet.Eve.OrderIoHelper.Models {
    [CollectionDataContract(Name = "DocumentElement", Namespace = "")]
    public class BuyOrderCollection : Collection<BuyOrder> {



    }
}