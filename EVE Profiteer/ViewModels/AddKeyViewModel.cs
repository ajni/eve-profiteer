using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels {
    public class AddKeyViewModel : Screen {

        public ApiKey Key { get; set; }

        private readonly EveApiService eveApi;

        private ICollection<ApiKeyEntity> entities;

        private readonly KeyManagementService keyManagementService;

        private readonly IWindowManager windowManager;

        public ICollection<ApiKeyEntity> Entities {
            get { return entities; }
            set { entities = value; NotifyOfPropertyChange(() => Entities); }
        }

        public AddKeyViewModel(IWindowManager windowManager, KeyManagementService keyManagementService, EveApiService eveApi) {
            this.windowManager = windowManager;
            this.keyManagementService = keyManagementService;
            this.eveApi = eveApi;
            Key = keyManagementService.ApiKeyRepository.Create();
            Key.ApiKeyId = 3053778;
            Key.VCode = "Hu3uslqNc3HDP8XmMMt1Cgb56TpPqqnF2tXssniROFkIMEDLztLPD8ktx6q5WVC2";
        }

        public void LoadButton() {
            Entities = eveApi.GetApiKeyEntities(Key);
        }

        public void SaveButton() {
            if (keyManagementService.ApiKeyRepository.All().SingleOrDefault(t => t.ApiKeyId == Key.ApiKeyId) != null) {
                MessageBox.Show("This key has already been added.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            keyManagementService.AddKey(Key, Entities);
            TryClose(true);
        }
    }
}
