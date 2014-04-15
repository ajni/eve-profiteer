using System.Linq;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Repository;
using eZet.EveProfiteer.Services;
using Xceed.Wpf.DataGrid;

namespace eZet.EveProfiteer.ViewModels {
    public class JournalViewModel : Screen {

        private readonly EveProfiteerDbContext dbContext;

        private readonly EveApiService eveApi;

        private DataGridCollectionView journal;

        public DataGridCollectionView Journal {
            get { return journal; }
            set { journal = value; NotifyOfPropertyChange(() => Journal); }
        }

        public ApiKey ApiKey { get; set; }

        public ApiKeyEntity Entity { get; set; }

        public JournalViewModel(EveProfiteerDbContext dbContext, EveApiService eveApi) {
            this.dbContext = dbContext;
            this.eveApi = eveApi;
            DisplayName = "Journal";
        }

        public void Initialize(ApiKey key, ApiKeyEntity entity) {
            ApiKey = key;
            Entity = entity;
            Journal = new DataGridCollectionView(dbContext.JournalEntries.Where(t => t.ApiKeyEntity.Id == entity.Id).ToList());
        }

        public void Update() {
            long latest = 0;
            latest = (from t in dbContext.JournalEntries
                      //where t.Entity.Id == Entity.Id
                      orderby t.RefId descending
                      select t.RefId).FirstOrDefault();
            var list = eveApi.GetNewJournalEntries(ApiKey, Entity, latest);
            dbContext.JournalEntries.AddRange(list);
            dbContext.SaveChanges();
        }

        public void FullRefresh() {
            dbContext.JournalEntries.RemoveRange(dbContext.JournalEntries.Where(i => i.ApiKeyEntity.Id == Entity.Id));
            var list = eveApi.GetAllJournalEntries(ApiKey, Entity, dbContext.JournalEntries.Create);
            dbContext.JournalEntries.AddRange(list);
            dbContext.SaveChanges();
        }

    }
}
