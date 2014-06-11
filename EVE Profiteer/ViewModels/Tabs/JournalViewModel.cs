using System.Linq;
using System.Windows.Input;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels.Tabs {
    public class JournalViewModel : Screen {
        private readonly JournalService _journalService;
        private IQueryable<JournalEntry> _journal;

        public JournalViewModel(JournalService journalService) {
            _journalService = journalService;
            DisplayName = "Journal";
        }


        protected override void OnActivate() {
            if (Journal == null)
                Journal = _journalService.GetJournal();
        }

        protected override void OnDeactivate(bool close) {
            Journal = null;
            _journalService.Deactivate();
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
    }
}
