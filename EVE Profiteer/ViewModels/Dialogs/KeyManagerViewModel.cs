using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels.Dialogs {
    public sealed class KeyManagerViewModel : Screen {
        private readonly KeyManagerService _keyManagerService;
        private readonly IWindowManager _windowManager;
        private BindableCollection<ApiKey> _keys;
        private ApiKey _selectedKey;

        public KeyManagerViewModel(IWindowManager windowManager, KeyManagerService keyManagerService) {
            _windowManager = windowManager;
            _keyManagerService = keyManagerService;
            DisplayName = "Manage Keys";
            Initialize = InitializeAsync();
        }

        public Task Initialize { get; private set; }

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

        private async Task InitializeAsync() {
            Keys = new BindableCollection<ApiKey>(await _keyManagerService.GetKeys().ConfigureAwait(false));
        }

        public async void AddKeyButton() {
            var vm = IoC.Get<AddKeyViewModel>();
            bool? result = _windowManager.ShowDialog(vm);
            if (!result.GetValueOrDefault()) return;
            if (Keys.Any(t => t.ApiKeyId == vm.Key.ApiKeyId)) {
                MessageBox.Show("Could not add key, the key has already been added.", "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            vm.Key.ApiKeyEntities = vm.Entities;
            await _keyManagerService.AddKey(vm.Key).ConfigureAwait(false);
            Keys.Add(vm.Key);
        }

        public async void EditKeyButton() {
            EditKeyViewModel vm = IoC.Get<EditKeyViewModel>().With(SelectedKey);
            if (_windowManager.ShowDialog(vm).GetValueOrDefault()) {
                await _keyManagerService.Update(SelectedKey, vm.Entities).ConfigureAwait(false);
                Refresh();
            }
        }

        public async void DeleteKeyButton() {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this key?", "Confirm delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result == MessageBoxResult.No) return;
            await _keyManagerService.DeleteKey(SelectedKey).ConfigureAwait(false);
            Keys.Remove(SelectedKey);
            SelectedKey = null;
        }
    }
}