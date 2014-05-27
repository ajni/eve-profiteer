using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.ViewModels {
    public class TransactionsViewModel : Screen {
        private readonly Services.TransactionService _transactionService;
        private ObservableCollection<Transaction> _transactions;

        public TransactionsViewModel(Services.TransactionService transactionService) {
            _transactionService = transactionService;
            DisplayName = "Transactions";
        }

        public ObservableCollection<Transaction> Transactions {
            get { return _transactions; }
            set {
                _transactions = value;
                NotifyOfPropertyChange(() => Transactions);
            }
        }

        public ApiKey ApiKey { get; set; }

        public ApiKeyEntity ApiKeyEntity { get; set; }
        public void Initialize(ApiKey key, ApiKeyEntity entity) {
            ApiKey = key;
            ApiKeyEntity = entity;
            //Transactions = new ObservableCollection<Transaction>(_dataService.Db.Transactions().Where(t => t.ApiKeyEntity.Id == entity.Id).ToList());
        }

        protected override void OnInitialize() {
        }

    }
}