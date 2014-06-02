using System.Linq;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels.Dialogs {
    public class ManageKeysViewModel : Screen {
        private readonly KeyManagementService _keyManagementService;
        private readonly IWindowManager _windowManager;
        private BindableCollection<ApiKey> _keys;
        private ApiKey _selectedKey;

        public ManageKeysViewModel(IWindowManager windowManager, KeyManagementService keyManagementService) {
            _windowManager = windowManager;
            _keyManagementService = keyManagementService;
            Keys = new BindableCollection<ApiKey>(keyManagementService.AllApiKeys().ToList());
        }

        public ApiKey SelectedKey {
            get { return _selectedKey; }
            set {
                _selectedKey = value;
                NotifyOfPropertyChange(() => _selectedKey);
                NotifyOfPropertyChange(() => CanEditKeyButton);
                NotifyOfPropertyChange(() => CanDeleteKeyButton);
            }
        }

        public bool CanEditKeyButton {
            get { return SelectedKey != null; }
        }

        public bool CanDeleteKeyButton {
            get { return SelectedKey != null; }
        }

        public BindableCollection<ApiKey> Keys {
            get { return _keys; }
            set {
                _keys = value;
                NotifyOfPropertyChange(() => Keys);
            }
        }

        public void AddKeyButton() {
            var vm = IoC.Get<AddKeyViewModel>();
            bool? result = _windowManager.ShowDialog(vm);
            if (result.HasValue && result == true) {
                Keys.Add(vm.Key);
            }
        }

        public void EditKeyButton() {
            EditKeyViewModel vm = IoC.Get<EditKeyViewModel>().With(SelectedKey);
            _windowManager.ShowDialog(vm);
        }

        public void DeleteKeyButton() {
            _keyManagementService.DeleteKey(SelectedKey);
            Keys.Remove(SelectedKey);
            SelectedKey = null;
        }
    }
}