using System.Linq;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;
using Xceed.Wpf.DataGrid;

namespace eZet.EveProfiteer.ViewModels {
    public class TransactionsViewModel : Screen {

        private DataGridCollectionView transactions;

        private readonly TransactionService transactionService;


        public DataGridCollectionView Transactions {
            get { return transactions; }
            set { transactions = value; NotifyOfPropertyChange(() => Transactions); }
        }

        public ApiKey ApiKey { get; set; }

        public ApiKeyEntity Entity { get; set; }

        private readonly EveApiService eveApi;

        public TransactionsViewModel(TransactionService transactionService, EveApiService eveApi) {
            this.transactionService = transactionService;
            this.eveApi = eveApi;
            DisplayName = "Transactions";
        }

        public void Initialize(ApiKey key, ApiKeyEntity entity) {
            ApiKey = key;
            Entity = entity;
            Transactions = new DataGridCollectionView(transactionService.All().Where(t => t.ApiKeyEntity.Id == entity.Id));
            //Update();
        }

        public void Update() {
            long latest = 0;
            latest = transactionService.GetLatestId();
            var list = eveApi.GetNewTransactions(ApiKey, Entity, transactionService.Create, latest);
            transactionService.AddRange(list);
            transactionService.SaveChanges();
        }

        public void FullRefresh() {
            transactionService.RemoveAll(Entity);
            var list = eveApi.GetAllTransactions(ApiKey, Entity, transactionService.Create);
            transactionService.AddRange(list);
            transactionService.SaveChanges();
        }
    }
}
