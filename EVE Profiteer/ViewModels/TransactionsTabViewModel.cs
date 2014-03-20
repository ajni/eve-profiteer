using System.Linq;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;
using Xceed.Wpf.DataGrid;

namespace eZet.EveProfiteer.ViewModels {
    public class TransactionsTabViewModel : Screen {

        private readonly EveProfiteerDbContext dbContext;

        private DataGridCollectionView transactions;


        public DataGridCollectionView Transactions {
            get { return transactions; }
            set { transactions = value; NotifyOfPropertyChange(() => Transactions); }
        }

        public ApiKey ApiKey { get; set; }

        public ApiKeyEntity Entity { get; set; }

        private readonly EveApiService eveApi;

        public TransactionsTabViewModel(EveProfiteerDbContext dbContext, EveApiService eveApi) {
            this.dbContext = dbContext;
            this.eveApi = eveApi;
            DisplayName = "Transactions";
        }

        public void Initialize(ApiKey key, ApiKeyEntity entity) {
            ApiKey = key;
            Entity = entity;
            Transactions = new DataGridCollectionView(dbContext.Transactions.Where(t => t.ApiKeyEntity.Id == entity.Id).ToList());
            Update();
        }

        public void Update() {
            long latest = 0;
            latest = (from t in dbContext.Transactions
                      //where t.Entity.Id == Entity.Id
                      orderby t.TransactionId descending
                      select t.TransactionId).FirstOrDefault();
            var list = eveApi.GetNewTransactions(ApiKey, Entity, dbContext.Transactions.Create, latest);
            dbContext.Transactions.AddRange(list);
            dbContext.SaveChanges();
        }

        public void FullRefresh() {
            dbContext.Transactions.RemoveRange(dbContext.Transactions.Where(i => i.ApiKeyEntity.Id == Entity.Id));
            var list = eveApi.GetAllTransactions(ApiKey, Entity, dbContext.Transactions.Create);
            dbContext.Transactions.AddRange(list);
            dbContext.SaveChanges();
        }
    }
}
