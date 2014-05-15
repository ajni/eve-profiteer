using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using DevExpress.Xpf.Ribbon.Customization;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels {
    public class TransactionsViewModel : Screen {
        private readonly EveApiService _eveApi;
        private readonly TransactionService _transactionService;
        private ObservableCollection<Transaction> _transactions;

        public TransactionsViewModel(TransactionService transactionService, EveApiService eveApi) {
            _transactionService = transactionService;
            _eveApi = eveApi;
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
            Transactions = new ObservableCollection<Transaction>(_transactionService.All().Where(t => t.ApiKeyEntity.Id == entity.Id).ToList());
        }

        protected override void OnInitialize() {
        }

        public async Task UpdateAction() {long latest = 0;
            latest = _transactionService.GetLatestId(ApiKeyEntity);
            //latest = 3436520013;
            IEnumerable<Transaction> list = _eveApi.GetNewTransactions(ApiKey, ApiKeyEntity, latest);
            var transactions = list as IList<Transaction> ?? list.ToList();
            Transactions.AddRange(transactions);
            await Task.Run(() => _transactionService.BulkInsert(transactions));
            //NotifyOfPropertyChange(() => Transactions);
        }

        public void FullRefresh() {
            _transactionService.RemoveAll(ApiKeyEntity);
            IEnumerable<Transaction> list = _eveApi.GetAllTransactions(ApiKey, ApiKeyEntity, _transactionService.Create);
            _transactionService.BulkInsert(list);
        }
    }
}