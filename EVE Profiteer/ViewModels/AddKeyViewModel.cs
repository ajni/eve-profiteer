using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels {
    public class AddKeyViewModel : Screen {
        private readonly EveApiService eveApi;

        private readonly KeyManagementService keyManagementService;
        private ICollection<ApiKeyEntity> entities;

        public AddKeyViewModel(KeyManagementService keyManagementService, EveApiService eveApi) {
            this.keyManagementService = keyManagementService;
            this.eveApi = eveApi;
            Key = keyManagementService.CreateApiKey();
            Key.ApiKeyId = 3053778;
            Key.VCode = "Hu3uslqNc3HDP8XmMMt1Cgb56TpPqqnF2tXssniROFkIMEDLztLPD8ktx6q5WVC2";
        }

        public ApiKey Key { get; set; }

        public ICollection<ApiKeyEntity> Entities {
            get { return entities; }
            set {
                entities = value;
                NotifyOfPropertyChange(() => Entities);
            }
        }

        public void LoadButton() {
            Entities = eveApi.GetApiKeyEntities(Key);
        }

        public void SaveButton() {
            if (keyManagementService.AllApiKeys().SingleOrDefault(t => t.ApiKeyId == Key.ApiKeyId) != null) {
                MessageBox.Show("This key has already been added.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
            keyManagementService.AddKey(Key, Entities);
            TryClose(true);
        }
    }
}