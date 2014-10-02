using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Services {
    public class TransactionDetailsService {
        private readonly Repository _repository;

        public TransactionDetailsService(Repository repository) {
            _repository = repository;
        }

        public async Task<List<InvType>> GetSelectableItems() {
            IQueryable<IGrouping<InvType, Transaction>> groups = _repository.MyTransactions().GroupBy(t => t.InvType);
            return await groups.AsNoTracking()
                    .Select(g => g.Key)
                    .OrderBy(t => t.TypeName)
                    .ToListAsync();
        }

        public async Task<List<Transaction>> GetTransactions(InvType type, DateTime start, DateTime end) {
            return await _repository.MyTransactions()
                .Where(f => f.TypeId == type.TypeId && f.TransactionDate >= start.Date && f.TransactionDate <= end.Date)
                .Include("InvType").ToListAsync();
        }

        public async Task<Order> GetOrder(InvType type) {
            return await _repository.MyOrders()
                .SingleOrDefaultAsync(t => t.TypeId == type.TypeId);
        }
    }
}
