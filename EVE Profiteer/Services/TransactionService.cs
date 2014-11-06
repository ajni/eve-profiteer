using System.Data.Entity;
using System.Linq;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Services {
    public class TransactionService {
        private readonly EveProfiteerRepository _eveProfiteerRepository;

        public TransactionService(EveProfiteerRepository eveProfiteerRepository) {
            _eveProfiteerRepository = eveProfiteerRepository;
        }

        public IQueryable<Transaction> GetTransactions() {
            return _eveProfiteerRepository.MyTransactions().Include("InvType");
        }


    }
}
