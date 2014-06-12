using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels.Tabs {
    public class JournalViewModel : Screen {
        private readonly JournalService _journalService;
        private IQueryable<JournalEntry> _journal;
        private JournalAggregate<DateTime> _dailyAggregate;
        private JournalAggregate<string> _typeAggregate;

        public JournalViewModel(JournalService journalService) {
            _journalService = journalService;
            DisplayName = "Journal";
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

        protected override void OnActivate() {
            if (Journal == null)
                Journal = _journalService.GetJournal();
        }

        protected override void OnDeactivate(bool close) {
            Journal = null;
            _journalService.Deactivate();
        }

        public async Task InitAsync() {
            List<JournalEntry> journal = await _journalService.GetJournal().AsNoTracking().ToListAsync().ConfigureAwait(false);
            IEnumerable<IGrouping<string, JournalEntry>> typeGroup = journal.GroupBy(t => t.RefType.Name);
            TypeAggregate = new JournalAggregate<string>(typeGroup);
            IEnumerable<IGrouping<DateTime, JournalEntry>> dateGroup =
                journal.GroupBy(t => t.Date.Date).OrderBy(t => t.Key);
            DailyAggregate = new JournalAggregate<DateTime>(dateGroup);
        }
    }
}