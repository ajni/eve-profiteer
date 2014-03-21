using System.Data.Entity;
using System.Linq;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Repository;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels {
    public class EditKeyViewModel : Screen {

        private ApiKey key;

        private bool isRefreshed;

        public ApiKey Key {
            get { return key; }
            set { key = value; NotifyOfPropertyChange(() => Key); }
        }

        private readonly EveApiService eveApi;

        private readonly EveProfiteerDbContext db;

        private BindableCollection<ApiKeyEntity> entities;

        public BindableCollection<ApiKeyEntity> Entities {
            get { return entities; }
            set { entities = value; NotifyOfPropertyChange(() => Entities); }
        }

        public EditKeyViewModel(EveProfiteerDbContext db, EveApiService eveApi) {
            this.db = db;
            this.eveApi = eveApi;
        }

        public EditKeyViewModel With(ApiKey apikey) {
            Key = apikey;
            Entities = new BindableCollection<ApiKeyEntity>(Key.Entities.ToList());
            return this;
        }


        public void RefreshButton() {
            Entities = new BindableCollection<ApiKeyEntity>(eveApi.GetApiKeyEntities(Key));
            isRefreshed = true;
        }

        // TODO Remove deleted characters
        public void SaveButton() {
            if (isRefreshed) {
                foreach (var entity in Entities) {
                    var a = Key.Entities.Single(e => e.EntityId == entity.EntityId);
                    if (a != null) {
                        a.IsActive = entity.IsActive;
                    }
                    else {
                        db.Entry(entity).State = EntityState.Added;
                    }
                }
            }
            db.SaveChanges();
            TryClose(true);
        }
    }
}
