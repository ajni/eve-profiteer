using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels.Modules {
    public class JournalViewModel : ModuleViewModel {
        private readonly JournalService _journalService;
        private IQueryable<JournalEntry> _journal;

        public JournalViewModel(JournalService journalService) {
            _journalService = journalService;
        }

        public IQueryable<JournalEntry> Journal {
            get { return _journal; }
            private set {
                if (Equals(value, _journal)) return;
                _journal = value;
                NotifyOfPropertyChange();
            }
        }

        public ICommand ViewTradeDetailsCommand { get; private set; }

        public ICommand ViewMarketDetailsCommand { get; private set; }

        public ICommand ViewOrderCommand { get; private set; }

        public ICommand ViewPeriodCommand { get; private set; }

        public ICommand AddToOrdersCommand { get; private set; }

        protected override Task OnActivate() {
            if (Journal == null) {
                Journal = _journalService.GetJournal();
            }
            return Task.FromResult(0);
        }

        protected override async Task OnDeactivate(bool close) {
            if (close) {
                Journal = null;
                await Initialized;
                _journalService.Deactivate();
            }
        }

    }
}