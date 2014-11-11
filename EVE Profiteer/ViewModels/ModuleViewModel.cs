using System.Threading.Tasks;
using Caliburn.Micro;
using DevExpress.Xpf.Docking;

namespace eZet.EveProfiteer.ViewModels {
    public abstract class ModuleViewModel : Screen, IMVVMDockingProperties {
        public new bool IsActive { get; private set; }
        public Task Initialized { get; protected set; }

        protected Task _open;
        protected Task _activate;


        public async Task Initialize() {
            if (Initialized == null) {
                Initialized = OnInitialize();
            }
            await Initialized.ConfigureAwait(false);
        }

        public async Task Open() {
            await Initialize().ConfigureAwait(false);
            if (_open == null) _open = OnOpen();
            await _open.ConfigureAwait(false);
        }

        public async Task Activate() {
            await Open().ConfigureAwait(false);
            if (_activate == null)
                _activate = OnActivate();
            await _activate.ConfigureAwait(false);
            IsActive = true;
        }

        public async Task Deactivate(bool close) {
            if (close)
                _open = null;
            await OnDeactivate(close).ConfigureAwait(false);
            IsActive = false;
        }


        protected virtual Task OnOpen() {
            return Task.FromResult(0);
        }

        protected virtual new Task OnActivate() {
            return Task.FromResult(0);
        }

        protected new virtual Task OnDeactivate(bool close) {
            return Task.FromResult(0);
        }

        protected new virtual Task OnInitialize() {
            return Task.FromResult(0);
        }

        private string _targetName = "ModuleHost";

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