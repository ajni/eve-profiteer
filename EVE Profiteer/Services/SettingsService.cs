using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Services {
    public class SettingsService : DbContextService {
        private readonly Repository _repository;

        public SettingsService(Repository repository) {
            _repository = repository;
        }

        public Task<List<MapRegion>> GetRegions() {
            return _repository.GetRegionsOrdered().Include("StaStations").ToListAsync();
        }
    }
}
