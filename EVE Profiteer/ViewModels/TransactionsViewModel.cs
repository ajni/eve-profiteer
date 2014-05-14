using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels {
    public class TransactionsViewModel : Screen {
        private readonly EveApiService eveApi;
        private readonly TransactionService transactionService;
        private ICollection<Transaction> transactions;

        public TransactionsViewModel(TransactionService transactionService, EveApiService eveApi) {
            this.transactionService = transactionService;
            this.eveApi = eveApi;
            DisplayName = "Transactions";
        }

        public ICollection<Transaction> Transactions {
            get { return transactions; }
            set {
                transactions = value;
                NotifyOfPropertyChange(() => Transactions);
            }
        }

        public ApiKey ApiKey { get; set; }

        public ApiKeyEntity Entity { get; set; }

        public void Initialize(ApiKey key, ApiKeyEntity entity) {
            ApiKey = key;
            Entity = entity;
            Transactions = transactionService.All().Where(t => t.ApiKeyEntity.Id == entity.Id).ToList();
        }

        public void UpdateAction() {
            long latest = 0;
            latest = transactionService.GetLatestId(Entity);
            IEnumerable<Transaction> list = eveApi.GetNewTransactions(ApiKey, Entity, latest);
            foreach (Transaction item in list) {
                Transactions.Add(item);
            }
            //NotifyOfPropertyChange(() => Transactions);
            //transactionService.AddNew(list);
        }

        public void FullRefresh() {
            transactionService.RemoveAll(Entity);
            IEnumerable<Transaction> list = eveApi.GetAllTransactions(ApiKey, Entity, transactionService.Create);
            transactionService.AddNew(list);
        }
    }
}