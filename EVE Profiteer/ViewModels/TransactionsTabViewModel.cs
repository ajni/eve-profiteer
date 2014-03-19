using System.Linq;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels {
    public class TransactionsTabViewModel : Screen {

        private IQueryable<Transaction> transactions;

        private readonly EveProfiteerDbContext dbContext;

        public IQueryable<Transaction> Transactions {
            get { return transactions; }
            set { transactions = value; NotifyOfPropertyChange(() => Transactions); }
        }

        public ApiKey ApiKey { get; set; }

        public ApiKeyEntity Entity { get; set; }

        private readonly EveApiService eveApiService;

        public TransactionsTabViewModel(EveProfiteerDbContext dbContext, EveApiService eveApiService) {
            this.dbContext = dbContext;
            this.eveApiService = eveApiService;
            DisplayName = "Transactions";
            Transactions = dbContext.Transactions;
        }

        public void Initialize(ApiKey key, ApiKeyEntity entity) {
            ApiKey = key;
            Entity = entity;
            Update();
        }

        public void Update() {
            long latest = 0;
            latest = (from t in dbContext.Transactions
                      //where t.Entity.Id == Entity.Id
                      orderby t.TransactionId descending
                      select t.TransactionId).FirstOrDefault();
            var list = eveApiService.GetNewTransactions(ApiKey, Entity, dbContext.Transactions.Create, latest);
            dbContext.Transactions.AddRange(list);
            dbContext.SaveChanges();
        }

        public void FullRefresh() {
            dbContext.Transactions.RemoveRange(dbContext.Transactions.Where(i => i.ApiKeyEntity.Id == Entity.Id));
            var list = eveApiService.GetAllTransactions(ApiKey, Entity, dbContext.Transactions.Create);
            dbContext.Transactions.AddRange(list);
            dbContext.SaveChanges();
        }




    }
}
