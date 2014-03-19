using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels {
    public class ManageKeysViewModel : Screen {

        private readonly IWindowManager windowManager;
        private readonly EveProfiteerDbContext dbContext;

        private BindableCollection<ApiKey> keys;

        private ApiKey selectedKey;

        public ApiKey SelectedKey {
            get { return selectedKey; }
            set { selectedKey = value; NotifyOfPropertyChange(() => selectedKey); NotifyOfPropertyChange(() => CanEditKeyButton); NotifyOfPropertyChange(() => CanDeleteKeyButton);}
        }


        public bool CanEditKeyButton {
            get { return SelectedKey != null; }
        }

        public bool CanDeleteKeyButton {
            get { return SelectedKey != null; }
        }

        public BindableCollection<ApiKey> Keys {
            get { return keys; }
            set { keys = value; NotifyOfPropertyChange(() => Keys); }
        }


        public ManageKeysViewModel(IWindowManager windowManager, EveProfiteerDbContext dbContext) {
            this.windowManager = windowManager;
            this.dbContext = dbContext;
            Keys = new BindableCollection<ApiKey>(dbContext.ApiKeys);
        }

        public void AddKeyButton() {
            var vm = IoC.Get<AddKeyViewModel>();
            var result = windowManager.ShowDialog(IoC.Get<AddKeyViewModel>());
            if (result.HasValue && result == true) {
                Keys.Add(vm.Key);
            }
        }

        public void EditKeyButton() {
            var vm = IoC.Get<EditKeyViewModel>().With(SelectedKey);
            windowManager.ShowDialog(vm);
        }


        public async void DeleteKeyButton() {
            dbContext.ApiKeys.Remove(SelectedKey);
            await dbContext.SaveChangesAsync();
            Keys.Remove(SelectedKey);
            SelectedKey = null;
        }
    }
}
