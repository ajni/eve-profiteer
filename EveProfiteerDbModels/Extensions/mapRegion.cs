using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace eZet.EveProfiteer.Models {
    public partial class MapRegion {

        public ICollection<StaStation> StationsAscending {
            get { return StaStations.OrderBy(station => station.StationName).ToList(); }
        }
    }
}
