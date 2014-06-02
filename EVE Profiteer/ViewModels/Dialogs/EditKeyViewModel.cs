using System.Linq;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels.Dialogs {
    public class EditKeyViewModel : Screen {
        private readonly EveApiService eveApi;
        private readonly KeyManagementService keyManagementService;

        private BindableCollection<ApiKeyEntity> entities;
        private bool isRefreshed;
        private ApiKey key;

        public EditKeyViewModel(KeyManagementService keyManagementService, EveApiService eveApi) {
            this.keyManagementService = keyManagementService;
            this.eveApi = eveApi;
        }

        public ApiKey Key {
            get { return key; }
            set {
                key = value;
                NotifyOfPropertyChange(() => Key);
            }
        }

        public BindableCollection<ApiKeyEntity> Entities {
            get { return entities; }
            set {
                entities = value;
                NotifyOfPropertyChange(() => Entities);
            }
        }

        public EditKeyViewModel With(ApiKey apikey) {
            Key = apikey;
            Entities = new BindableCollection<ApiKeyEntity>(Key.ApiKeyEntities.ToList());
            return this;
        }


        public void RefreshButton() {
            Entities = new BindableCollection<ApiKeyEntity>(eveApi.GetApiKeyEntities(Key));
            isRefreshed = true;
        }

        // TODO Remove deleted characters
        public void SaveButton() {
            if (isRefreshed) {
                foreach (ApiKeyEntity entity in Entities) {
                    ApiKeyEntity a = Key.ApiKeyEntities.Single(e => e.EntityId == entity.EntityId);
                    if (a != null) {
                        a.IsActive = entity.IsActive;
                    }
                }
            }
            //db.SaveChanges();
            TryClose(true);
        }
    }
}