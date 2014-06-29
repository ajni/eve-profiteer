using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using eZet.EveProfiteer.Models;
namespace eZet.EveProfiteer.Services {
    public class TradeSummaryService {
        private readonly Repository _repository;

        public TradeSummaryService(Repository repository) {
            _repository = repository;
        }

        public Task<List<Transaction>> GetTransactions(DateTime start, DateTime end) {
            return _repository.MyTransactions().Where(t => t.TransactionDate >= start.Date && t.TransactionDate <= end.Date)
                         .ToListAsync();
        }
    }
}