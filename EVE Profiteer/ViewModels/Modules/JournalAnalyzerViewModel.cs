using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels.Modules {
    public class JournalAnalyzerViewModel : ModuleViewModel {
        private readonly JournalService _journalService;
        private IQueryable<JournalEntry> _journal;
        private JournalAggregate<DateTime> _dailyAggregate;
        private JournalAggregate<string> _typeAggregate;

        public JournalAnalyzerViewModel(JournalService journalService) {
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

        public JournalAggregate<DateTime> DailyAggregate {
            get { return _dailyAggregate; }
            set {
                if (Equals(value, _dailyAggregate)) return;
                _dailyAggregate = value;
                NotifyOfPropertyChange();
            }
        }

        public JournalAggregate<string> TypeAggregate {
            get { return _typeAggregate; }
            set {
                if (Equals(value, _typeAggregate)) return;
                _typeAggregate = value;
                NotifyOfPropertyChange();
            }
        }


        public ICommand ViewTradeDetailsCommand { get; private set; }

        public ICommand ViewMarketDetailsCommand { get; private set; }

        public ICommand ViewOrderCommand { get; private set; }

        public ICommand ViewPeriodCommand { get; private set; }

        public ICommand AddToOrdersCommand { get; private set; }

        protected override Task OnActivate() {
            //_journalService.Activate();
            if (Journal == null) {
                Journal = _journalService.GetJournal();
            }
            return Task.FromResult(0);
        }

        protected override async Task OnDeactivate(bool close) {
            Journal = null;
            await Initialized;
            _journalService.Deactivate();
            if (close) {
                DailyAggregate = null;
                TypeAggregate = null;
                Initialized = null;
            }
        }

        private Task InitializeAsync() {
            //_journalService.Activate();
            //List<JournalEntry> journal =
            //    await _journalService.GetJournal().ToListAsync().ConfigureAwait(false);
            //IEnumerable<IGrouping<string, JournalEntry>> typeGroup = journal.GroupBy(t => t.RefType.Name);
            //TypeAggregate = new JournalAggregate<string>(typeGroup);
            //IEnumerable<IGrouping<DateTime, JournalEntry>> dateGroup =
            //    journal.GroupBy(t => t.Date.Date).OrderBy(t => t.Key);
            //DailyAggregate = new JournalAggregate<DateTime>(dateGroup);
            return Task.FromResult(0);
        }
    }
}