using System.Linq;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels {
    public class ManageKeysViewModel : Screen {
        private readonly KeyManagementService keyManagementService;
        private readonly IWindowManager windowManager;
        private BindableCollection<ApiKey> keys;
        private ApiKey selectedKey;

        public ManageKeysViewModel(IWindowManager windowManager, KeyManagementService keyManagementService) {
            this.windowManager = windowManager;
            this.keyManagementService = keyManagementService;
            Keys = new BindableCollection<ApiKey>(keyManagementService.AllApiKeys().ToList());
        }

        public ApiKey SelectedKey {
            get { return selectedKey; }
            set {
                selectedKey = value;
                NotifyOfPropertyChange(() => selectedKey);
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
            get { return keys; }
            set {
                keys = value;
                NotifyOfPropertyChange(() => Keys);
            }
        }

        public void AddKeyButton() {
            var vm = IoC.Get<AddKeyViewModel>();
            bool? result = windowManager.ShowDialog(vm);
            if (result.HasValue && result == true) {
                Keys.Add(vm.Key);
            }
        }

        public void EditKeyButton() {
            EditKeyViewModel vm = IoC.Get<EditKeyViewModel>().With(SelectedKey);
            windowManager.ShowDialog(vm);
        }

        public void DeleteKeyButton() {
            keyManagementService.DeleteKey(SelectedKey);
            Keys.Remove(SelectedKey);
            SelectedKey = null;
        }
    }
}