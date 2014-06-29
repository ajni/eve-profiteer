using System.Data.Entity;
using System.Linq;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Services {
    public class TransactionService {
        private readonly Repository _repository;

        public TransactionService(Repository repository) {
            _repository = repository;
        }

        public IQueryable<Transaction> GetTransactions() {
            return _repository.MyTransactions().Include("InvType");
        }


    }
}
