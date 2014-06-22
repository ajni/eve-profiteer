using System.Threading.Tasks;
using Caliburn.Micro;
using DevExpress.Xpf.Docking;

namespace eZet.EveProfiteer.ViewModels {
    public abstract class ModuleViewModel : Screen, IMVVMDockingProperties {
        private string _targetName = "ModuleHost";

        public abstract Task InitAsync();

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