using System.Collections.Generic;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels {
    public class AddKeyViewModel : PropertyChangedBase {

        public int KeyId { get; set; }

        public string VCode { get; set; }

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
        }

        public void LoadButton() {
            Characters = eveApi.GetEntities(KeyId, VCode);
        }

        public void SaveButton() {
            db.ApiKeyEntities.AddRange(Characters);
            db.SaveChanges();
        }


    }
}
