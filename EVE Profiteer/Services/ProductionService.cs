using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;

namespace eZet.EveProfiteer.Services {
    public class ProductionService : IDisposable {
        private readonly EveProfiteerDbEntities _db;
        private readonly EveApiService _eveApiService;

        public ProductionService(EveProfiteerDbEntities db, EveApiService eveApiService) {
            _db = db;
            _eveApiService = eveApiService;
        }

        public async Task<IEnumerable<ProductionBatchEntry>> GetProductionBatches() {
            var list = await _db.ProductionBatches.Where(batch => batch.ApiKeyEntityId == ApplicationHelper.ActiveEntity.Id)
                    .ToListAsync().ConfigureAwait(false);
            return list.Select(batch => new ProductionBatchEntry(batch));
        }



        public void Dispose() {
            _db.Dispose();
        }
    }
}
