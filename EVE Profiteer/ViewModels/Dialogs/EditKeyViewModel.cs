using System.Linq;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels.Dialogs {
    public sealed class EditKeyViewModel : Screen {
        private readonly EveApiService _eveApi;

        private BindableCollection<ApiKeyEntity> _entities;
        private ApiKey _key;

        public EditKeyViewModel(EveApiService eveApi) {
            _eveApi = eveApi;
            DisplayName = "Edit Key";
        }

        public ApiKey Key {
            get { return _key; }
            set {
                _key = value;
                NotifyOfPropertyChange(() => Key);
            }
        }

        public BindableCollection<ApiKeyEntity> Entities {
            get { return _entities; }
            set {
                _entities = value;
                NotifyOfPropertyChange(() => Entities);
            }
        }

        public EditKeyViewModel With(ApiKey apikey) {
            Key = apikey;
            Entities = new BindableCollection<ApiKeyEntity>(Key.ApiKeyEntities.ToList());
            return this;
        }


        public void RefreshButton() {
            Entities = new BindableCollection<ApiKeyEntity>(_eveApi.GetApiKeyEntities(Key));
        }
    }
}