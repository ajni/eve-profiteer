using Caliburn.Micro;
using eZet.EveProfiteer.Framework;

namespace eZet.EveProfiteer.ViewModels {
    public class ShellViewModel : IShell {

        private readonly IWindowManager windowManager;

        public ShellViewModel(IWindowManager windowManager) {
            this.windowManager = windowManager;
        }

        public void ManageKeys() {
            var dialog = windowManager.ShowDialog(IoC.Get<ManageKeysViewModel>());
        }



    }
}
