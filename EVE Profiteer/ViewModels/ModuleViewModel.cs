using System.Drawing;
using System.Threading.Tasks;
using Caliburn.Micro;
using DevExpress.Xpf.Docking;

namespace eZet.EveProfiteer.ViewModels {
    public abstract class ModuleViewModel : Screen, IMVVMDockingProperties {

        public enum ModuleCategory {
            Basic,
            Trade,
            Industry,
        }

        private string _targetName = "ModuleHost";

        protected ModuleViewModel() {
            Hint = DisplayName;
            Glyph = DevExpress.Images.ImageResourceCache.Default.GetImage("images/grid/grid_16x16.png");
            LargeGlyph = DevExpress.Images.ImageResourceCache.Default.GetImage("images/grid/grid_32x32.png");
        }

        public ModuleCategory Category { get; protected set; }

        public Image Glyph { get; protected set; }

        public Image LargeGlyph { get; protected set; }


        public string Hint { get; private set; }

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