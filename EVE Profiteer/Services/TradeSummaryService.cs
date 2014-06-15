using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using eZet.EveProfiteer.Models;
namespace eZet.EveProfiteer.Services {
    public class TradeSummaryService : CurrentEntityService {
        public async Task<List<Transaction>> GetTransactions(DateTime start, DateTime end) {
            using (var db = CreateDb()) {
                return await
                    MyTransactions(db)
                        .Where(t => t.TransactionDate >= start.Date && t.TransactionDate <= end.Date)
                        .ToListAsync();
            }
        }
    }
}