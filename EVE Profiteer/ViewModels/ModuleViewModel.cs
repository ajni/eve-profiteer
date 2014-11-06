using System.Threading.Tasks;
using Caliburn.Micro;
using DevExpress.Xpf.Docking;
using eZet.EveProfiteer.Ui.Events;

namespace eZet.EveProfiteer.ViewModels {
    public abstract class ModuleViewModel : Screen, IMVVMDockingProperties {


        protected override async void OnInitialize() {
            await Initialize.ConfigureAwait(false);
        }
        protected override async void OnActivate() {
            await Initialize.ConfigureAwait(false);
        }

        public Task Initialize { get; protected set; }

        protected virtual Task InitializeAsync() {
            return Task.FromResult(0);
        }

        private string _targetName = "ModuleHost";

        public bool IsReady { get; set; }


        public string TargetName {
            get { return _targetName; }
            set {
                if (value == _targetName) return;
                _targetName = value;
                NotifyOfPropertyChange();
            }
        }
    }

}