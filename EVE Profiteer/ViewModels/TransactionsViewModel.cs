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
            Transactions = new ObservableCollection<Transaction>(_transactionService.Transactions().Where(t => t.ApiKeyEntity.Id == entity.Id).ToList());
        }

        protected override void OnInitialize() {
        }

    }
}