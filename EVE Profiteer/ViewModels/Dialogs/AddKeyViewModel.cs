using System.Collections.Generic;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels.Dialogs {
    public sealed class AddKeyViewModel : Screen {
        private readonly EveApiService eveApi;

        private ICollection<ApiKeyEntity> entities;

        public AddKeyViewModel(EveApiService eveApi) {
            this.eveApi = eveApi;
            DisplayName = "Add New Key";
            Key = new ApiKey();
        }

        public ApiKey Key { get; set; }

        public ICollection<ApiKeyEntity> Entities {
            get { return entities; }
            set {
                entities = value;
                NotifyOfPropertyChange();
            }
        }

        public void LoadButton() {
            Entities = eveApi.GetApiKeyEntities(Key);
        }
    }
}