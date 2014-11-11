using System.Threading.Tasks;
using Caliburn.Micro;

namespace eZet.EveProfiteer.ViewModels {
    public class ModuleHost : Screen {
        private BindableCollection<ModuleViewModel> modules;

        public ModuleHost() {
            Modules = new BindableCollection<ModuleViewModel>();
        }

        public ModuleViewModel ActiveModel { get; set; }

        public BindableCollection<ModuleViewModel> Modules {
            get { return modules; }
            set {
                if (Equals(value, modules)) return;
                modules = value;
                NotifyOfPropertyChange();
            }
        }


        public async Task Activate(ModuleViewModel model) {
            if (ActiveModel != null) {
                await Deactivate(ActiveModel);
            }
            if (!Modules.Contains(model)) {
                Modules.Add(model);
            }
            await model.Activate();
            ActiveModel = model;
        }

        public async Task Deactivate(ModuleViewModel model) {
            await model.Deactivate(false).ConfigureAwait(false);
            ActiveModel = null;
        }

        public async Task Close(ModuleViewModel model) {
            Modules.Remove(model);
            await model.Deactivate(true).ConfigureAwait(false);
            ActiveModel = null;

        }
    }
}
