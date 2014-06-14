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

namespace eZet.EveProfiteer.ViewModels.Tabs {
    public class TradeSummaryViewModel : Screen {
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
        private readonly IEventAggregator _eventAggregator;
        private ViewPeriodEnum _selectedViewPeriod;
        private TransactionAggregate _summary;


        public TradeSummaryViewModel(EveProfiteerDataService dataService, IEventAggregator eventAggregator) {
            _dataService = dataService;
            _eventAggregator = eventAggregator;
            DisplayName = "Trade summary";
            PeriodSelectorStart = DateTime.UtcNow.AddMonths(-1);
            PeriodSelectorEnd = DateTime.UtcNow;
            _selectedViewPeriod = ViewPeriodEnum.Month;
            ViewPeriodCommand = new DelegateCommand(ExecuteViewPeriod);
        }

        public ICommand ViewPeriodCommand { get; private set; }

        public DateTime ActualViewStart { get; private set; }

        public DateTime ActualViewEnd { get; private set; }

        public DateTime PeriodSelectorStart { get; set; }

        public DateTime PeriodSelectorEnd { get; set; }

        public IEnumerable<ViewPeriodEnum> ViewPeriodValues {
            get { return Enum.GetValues(typeof (ViewPeriodEnum)).Cast<ViewPeriodEnum>(); }
        }

        public ViewPeriodEnum SelectedViewPeriod {
            get { return _selectedViewPeriod; }
            set {
                _selectedViewPeriod = value;
                NotifyOfPropertyChange(() => SelectedViewPeriod);
            }
        }

        public TransactionAggregate Summary {
            get { return _summary; }
            private set {
                if (Equals(value, _summary)) return;
                _summary = value;
                NotifyOfPropertyChange(() => Summary);
            }
        }

        public async Task InitAsync() {
            await ViewPeriod();
        }

        public async void ExecuteViewPeriod() {
            await ViewPeriod();
        }

        public async Task ViewPeriod() {
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
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Loading..."));
            List<Transaction> transactions =
                await
                    _dataService.Db.Transactions.AsNoTracking()
                        .Where(t => t.TransactionDate >= start.Date && t.TransactionDate <= end.Date)
                        .ToListAsync();
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Analyzing..."));
            Summary = new TransactionAggregate(transactions.GroupBy(t => t.TransactionDate.Date));
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Analysis complete"));
        }
    }
}