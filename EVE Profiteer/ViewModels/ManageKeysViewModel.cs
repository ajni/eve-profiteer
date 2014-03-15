using System.Collections.Generic;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.ViewModels {
    public class ManageKeysViewModel : PropertyChangedBase {

        private readonly IWindowManager windowManager;

        private ICollection<ApiKey> keys;

        public ICollection<ApiKey> Keys {
            get { return keys; }
            set { keys = value; NotifyOfPropertyChange(() => Keys); }
        }


        public ManageKeysViewModel(IWindowManager windowManager) {
            this.windowManager = windowManager;
        }

        public void AddKeyButton() {
            windowManager.ShowDialog(IoC.Get<AddKeyViewModel>());
        }
    }
}
