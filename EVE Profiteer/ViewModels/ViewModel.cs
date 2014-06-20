using System.Threading.Tasks;
using Caliburn.Micro;

namespace eZet.EveProfiteer.ViewModels {
    public abstract class ViewModel : Screen {

        public abstract Task InitAsync();

    }
}
