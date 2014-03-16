using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels {
    public class AddKeyViewModel : Screen {

        public ApiKey Key { get; set; }

        private readonly EveApiService eveApi;

        private readonly KeyManagementDbContext db;

        private IList<ApiKeyEntity> characters;

        public IList<ApiKeyEntity> Characters {
            get { return characters; }
            set { characters = value; NotifyOfPropertyChange(() => Characters); }
        }

        public AddKeyViewModel(KeyManagementDbContext db, EveApiService eveApi) {
            this.db = db;
            this.eveApi = eveApi;
            Key = new ApiKey { ApiKeyId = 3053778, VCode = "Hu3uslqNc3HDP8XmMMt1Cgb56TpPqqnF2tXssniROFkIMEDLztLPD8ktx6q5WVC2" };
        }

        public void LoadButton() {
            Characters = eveApi.GetEntities(Key.ApiKeyId, Key.VCode);
        }

        public void SaveButton() {
            db.ApiKeyEntities.AddRange(Characters);
            db.ApiKeys.Add(Key);
            db.SaveChanges();
            TryClose(true);
        }
    }
}
