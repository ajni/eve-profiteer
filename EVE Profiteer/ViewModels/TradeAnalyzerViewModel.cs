using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using DevExpress.Xpf.Mvvm;
using eZet.EveProfiteer.Events;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;
using eZet.EveProfiteer.Util;

namespace eZet.EveProfiteer.ViewModels {
    public class TradeAnalyzerViewModel : Screen {
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
        private readonly IWindowManager _windowMananger;
        private ViewPeriodEnum _selectedViewPeriod;

        public TradeAnalyzerViewModel(IWindowManager windowMananger, IEventAggregator eventAggregator,
            EveProfiteerDataService dataService) {
            _windowMananger = windowMananger;
            _eventAggregator = eventAggregator;
            _dataService = dataService;
            DisplayName = "Trade Analyzer";
            Items = new BindableCollection<TradeTypeAggregate>();
            ViewTradeDetailsCommand = new DelegateCommand<TradeTypeAggregate>(ExecuteViewTradeDetails,
                CanViewTradeDetails);
            ViewMarketDetailsCommand =
                new DelegateCommand<TradeTypeAggregate>(
                    item => _eventAggregator.Publish(new ViewMarketDetailsEventArgs(item.InvType)), item => item != null);
            ViewPeriodCommand = new DelegateCommand(ViewPeriod);
            ViewOrderCommand = new DelegateCommand<TradeTypeAggregate>(ExecuteViewOrder, CanViewOrder);
            PeriodSelectorStart = DateTime.UtcNow.AddMonths(-1);
            PeriodSelectorEnd = DateTime.UtcNow;
        }

        public DateTime ActualViewStart { get; private set; }

        public DateTime ActualViewEnd { get; private set; }

        public DateTime PeriodSelectorStart { get; set; }

        public DateTime PeriodSelectorEnd { get; set; }

        public ICommand ViewTradeDetailsCommand { get; private set; }

        public ICommand ViewMarketDetailsCommand { get; private set; }

        public ICommand ViewOrderCommand { get; private set; }

        public ICommand ViewPeriodCommand { get; private set; }


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

        public BindableCollection<TradeTypeAggregate> Items { get; private set; }


        public int CustomDaySpan { get; set; }

        private bool CanViewOrder(TradeTypeAggregate tradeTypeAggregate) {
            return tradeTypeAggregate != null && tradeTypeAggregate.Order != null;
        }

        private void ExecuteViewOrder(TradeTypeAggregate entry) {
            _eventAggregator.Publish(new ViewOrderEventArgs(entry.InvType));
        }

        private bool CanViewTradeDetails(TradeTypeAggregate tradeTypeAggregate) {
            return tradeTypeAggregate != null && tradeTypeAggregate.Order != null;
        }

        private void ExecuteViewTradeDetails(TradeTypeAggregate tradeTypeAggregate) {
            if (tradeTypeAggregate != null) {
                _eventAggregator.Publish(new ViewTradeDetailsEventArgs(tradeTypeAggregate.InvType));
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
            await Task.Run(() => analyze(ActualViewStart, ActualViewEnd));
        }

        private void analyze(DateTime start, DateTime end) {
            _eventAggregator.Publish(new StatusChangedEventArgs("Analyzing..."));

            Items.IsNotifying = false;
            Items.Clear();
            IQueryable<IGrouping<int, Transaction>> transactionGroups = _dataService.Db.Transactions.Where(
                t =>
                    t.TransactionDate >= start && t.TransactionDate <= end &&
                    t.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id)
                .GroupBy(t => t.TypeId);
            ILookup<int, Order> orders =
                _dataService.Db.Orders.Where(order => order.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id)
                    .ToLookup(order => order.TypeId);
            foreach (var transactionCollection in transactionGroups) {
                Items.Add(new TradeTypeAggregate(transactionCollection.First().InvType, transactionCollection.ToList(),
                    orders[transactionCollection.First().TypeId].SingleOrDefault()));
            }
            Items.IsNotifying = true;
            Items.Refresh();
            _eventAggregator.Publish(new StatusChangedEventArgs("Analysis complete"));

        }
    }
}