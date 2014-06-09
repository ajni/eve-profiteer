using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;

namespace eZet.EveProfiteer.Services {
    public class ProductionService {
        private readonly EveProfiteerDbEntities _db;

        public ProductionService(EveProfiteerDbEntities db) {
            _db = db;
        }

        public async Task<IEnumerable<ProductionBatchEntry>> GetProductionBatches() {
            var list = await _db.ProductionBatches.Where(batch => batch.ApiKeyEntityId == ApplicationHelper.ActiveKeyEntity.Id)
                    .ToListAsync().ConfigureAwait(false);
            return list.Select(batch => new ProductionBatchEntry(batch));
        }


    }
}
