using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Services {
    public class SettingsService : DbContextService {
        private readonly EveProfiteerRepository _eveProfiteerRepository;

        public SettingsService(EveProfiteerRepository eveProfiteerRepository) {
            _eveProfiteerRepository = eveProfiteerRepository;
        }

        public Task<List<MapRegion>> GetRegions() {
            return _eveProfiteerRepository.GetRegionsOrdered().Include("StaStations").ToListAsync();
        }
    }
}
