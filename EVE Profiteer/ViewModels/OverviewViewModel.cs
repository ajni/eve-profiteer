using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using DevExpress.Xpf.Mvvm;
using eZet.EveProfiteer.Events;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels {
    public class OverviewViewModel : Screen {

        public enum ViewPeriodEnum {
            Today,
            Yesterday,
            Week,
            Month,
            All,
            Since,
            Period
        }

        private readonly EveProfiteerDataService _dataService;
        private TransactionAggregateSummary _summary;
        private ViewPeriodEnum _selectedViewPeriod;
        private readonly IEventAggregator _eventAggregator;


        public OverviewViewModel(EveProfiteerDataService dataService, IEventAggregator eventAggregator) {
            _dataService = dataService;
            _eventAggregator = eventAggregator;
            DisplayName = "Trade summary";
            PeriodSelectorStart = DateTime.UtcNow.AddMonths(-1);
            PeriodSelectorEnd = DateTime.UtcNow;
            ViewPeriodCommand = new DelegateCommand(ViewPeriod);
        }

        public ICommand ViewPeriodCommand { get; private set; }

        public DateTime ActualViewStart { get; private set; }

        public DateTime ActualViewEnd { get; private set; }

        public DateTime PeriodSelectorStart { get; set; }

        public DateTime PeriodSelectorEnd { get; set; }

        public IEnumerable<ViewPeriodEnum> ViewPeriodValues {
            get { return Enum.GetValues(typeof(ViewPeriodEnum)).Cast<ViewPeriodEnum>(); }
        }

        public ViewPeriodEnum SelectedViewPeriod {
            get { return _selectedViewPeriod; }
            set {
                _selectedViewPeriod = value;
                NotifyOfPropertyChange(() => SelectedViewPeriod);
            }
        }



        protected async override void OnInitialize() {
            var transactions = await _dataService.Db.Transactions.AsNoTracking().ToListAsync();
            Summary = new TransactionAggregateSummary(transactions);
        }

        public TransactionAggregateSummary Summary {
            get { return _summary; }
            private set {
                if (Equals(value, _summary)) return;
                _summary = value;
                NotifyOfPropertyChange(() => Summary);
            }
        }

        public async void ViewPeriod() {
            ActualViewEnd = DateTime.UtcNow;
            switch (SelectedViewPeriod) {
                case ViewPeriodEnum.All:
                    ActualViewStart = DateTime.MinValue;
                    ActualViewEnd = DateTime.MaxValue;
                    break;
                case ViewPeriodEnum.Today:
                    ActualViewStart = DateTime.UtcNow.Date;
                    ActualViewEnd = DateTime.MaxValue;
                    break;
                case ViewPeriodEnum.Yesterday:
                    ActualViewStart = DateTime.UtcNow.AddDays(-1).Date;
                    ActualViewEnd = DateTime.UtcNow.Date;
                    break;
                case ViewPeriodEnum.Week:
                    ActualViewStart = DateTime.UtcNow.AddDays(-7);
                    ActualViewEnd = DateTime.MaxValue;
                    break;
                case ViewPeriodEnum.Month:
                    ActualViewStart = DateTime.UtcNow.AddMonths(-1);
                    ActualViewEnd = DateTime.MaxValue;
                    break;
                case ViewPeriodEnum.Since:
                    ActualViewStart = PeriodSelectorStart;
                    ActualViewEnd = DateTime.MaxValue;
                    break;
                case ViewPeriodEnum.Period:
                    ActualViewStart = PeriodSelectorStart;
                    ActualViewEnd = PeriodSelectorEnd;
                    break;
            }
            await analyze(ActualViewStart, ActualViewEnd);
        }

        private async Task analyze(DateTime start, DateTime end) {
            _eventAggregator.Publish(new StatusChangedEventArgs("Loading..."));
            var transactions = await _dataService.Db.Transactions.AsNoTracking().Where(t => t.TransactionDate > start && t.TransactionDate < end).ToListAsync();
            _eventAggregator.Publish(new StatusChangedEventArgs("Analyzing..."));
            Summary = new TransactionAggregateSummary(transactions);
            _eventAggregator.Publish(new StatusChangedEventArgs("Analysis complete"));

        }
    }
}