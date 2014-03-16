using System.Collections.Generic;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels {
    public class EditKeyViewModel : Screen {

        private ApiKey key;

        public ApiKey Key {
            get { return key; }
            set { key = value; NotifyOfPropertyChange(() => Key); }
        }

        private readonly EveApiService eveApi;

        private readonly KeyManagementDbContext db;

        private BindableCollection<ApiKeyEntity> characters;

        public BindableCollection<ApiKeyEntity> Characters {
            get { return characters; }
            set { characters = value; NotifyOfPropertyChange(() => Characters); }
        }

        public EditKeyViewModel(KeyManagementDbContext db, EveApiService eveApi) {
            this.db = db;
            this.eveApi = eveApi;
        }

        public EditKeyViewModel With(ApiKey apikey) {
            Key = apikey;
            var a = db.ApiKeys.Find(Key.ApiKeyId);
            Characters = new BindableCollection<ApiKeyEntity>(a.Entities);
            return this;
        }


        public void RefreshButton() {
            Characters = new BindableCollection<ApiKeyEntity>(eveApi.GetEntities(Key.ApiKeyId, Key.VCode));
        }

        public void SaveButton() {
            db.SaveChanges();
            TryClose(true);
        }
    }
}
