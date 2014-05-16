using System.Linq;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Repository;

namespace eZet.EveProfiteer.Services {
    public class TradeAnalyzerService {
        private readonly IRepository<Transaction> _transactionRepository;
        private readonly IRepository<Order> _orderRepository;

        public TradeAnalyzerService(IRepository<Transaction> transactionRepository, IRepository<Order> orderRepository) {
            _transactionRepository = transactionRepository;
            _orderRepository = orderRepository;
        }

        public IQueryable<Transaction> Transactions() {
            return _transactionRepository.Queryable();
        }

        public IQueryable<Order> Orders() {
            return _orderRepository.Queryable();
        }




    }
}
