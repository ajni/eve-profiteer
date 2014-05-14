using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;
using Xceed.Wpf.DataGrid;

namespace eZet.EveProfiteer.ViewModels {
    public class JournalViewModel : Screen {
        private readonly EveProfiteerDbEntities _dbEntities;

        private readonly EveApiService eveApi;

        private DataGridCollectionView journal;

        public JournalViewModel(EveProfiteerDbEntities _dbEntities, EveApiService eveApi) {
            this._dbEntities = _dbEntities;
            this.eveApi = eveApi;
            DisplayName = "Journal";
        }

        public DataGridCollectionView Journal {
            get { return journal; }
            set {
                journal = value;
                NotifyOfPropertyChange(() => Journal);
            }
        }

        public ApiKey ApiKey { get; set; }

        public ApiKeyEntity Entity { get; set; }

        public void Initialize(ApiKey key, ApiKeyEntity entity) {
            ApiKey = key;
            Entity = entity;
            Journal =
                new DataGridCollectionView(
                    _dbEntities.JournalEntries.Where(t => t.ApiKeyEntity.Id == entity.Id).ToList());
        }

        public void Update() {
            long latest = 0;
            latest = (from t in _dbEntities.JournalEntries
                //where t.Entity.Id == Entity.Id
                orderby t.RefId descending
                select t.RefId).FirstOrDefault();
            IList<JournalEntry> list = eveApi.GetNewJournalEntries(ApiKey, Entity, latest);
            _dbEntities.JournalEntries.AddRange(list);
            _dbEntities.SaveChanges();
        }

        public void FullRefresh() {
            _dbEntities.JournalEntries.RemoveRange(_dbEntities.JournalEntries.Where(i => i.ApiKeyEntity.Id == Entity.Id));
            IList<JournalEntry> list = eveApi.GetAllJournalEntries(ApiKey, Entity, _dbEntities.JournalEntries.Create);
            _dbEntities.JournalEntries.AddRange(list);
            _dbEntities.SaveChanges();
        }
    }
}