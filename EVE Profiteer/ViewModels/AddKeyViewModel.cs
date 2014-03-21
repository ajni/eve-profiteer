using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Repository;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels {
    public class AddKeyViewModel : Screen {

        public ApiKey Key { get; set; }

        private readonly EveApiService eveApi;

        private readonly EveProfiteerDbContext db;

        private ICollection<ApiKeyEntity> entities;

        private readonly ApiKeyService apiKeyService;

        private readonly IWindowManager windowManager;

        public ICollection<ApiKeyEntity> Entities {
            get { return entities; }
            set { entities = value; NotifyOfPropertyChange(() => Entities); }
        }

        public AddKeyViewModel(IWindowManager windowManager, ApiKeyService apiKeyService, EveProfiteerDbContext db, EveApiService eveApi) {
            this.windowManager = windowManager;
            this.apiKeyService = apiKeyService;
            this.db = db;
            this.eveApi = eveApi;
            Key = apiKeyService.ApiKeyRepository.Create();
            Key.ApiKeyId = 3053778;
            Key.VCode = "Hu3uslqNc3HDP8XmMMt1Cgb56TpPqqnF2tXssniROFkIMEDLztLPD8ktx6q5WVC2";
        }

        public void LoadButton() {
            Entities = eveApi.GetApiKeyEntities(Key);
        }

        public void SaveButton() {
            if (apiKeyService.ApiKeyRepository.Find(t => t.ApiKeyId == Key.ApiKeyId) != null) {
                MessageBox.Show("This key has already been added.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            apiKeyService.AddKey(Key, Entities);
            TryClose(true);
        }
    }
}
