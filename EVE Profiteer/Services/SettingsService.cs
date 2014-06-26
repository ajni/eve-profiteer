using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Services {
    public class SettingsService : DbContextService {

        private EveProfiteerDbEntities _db;

        public SettingsService() {
        }


        public async Task<List<MapRegion>> GetRegions() {
            using (var db = CreateDb()) {
                return await db.MapRegions.AsNoTracking().Include("StaStations").ToListAsync().ConfigureAwait(false);
            }
        }
    }
}
