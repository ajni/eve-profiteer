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
            Items = new BindableCollection<TradeAggregate>();
            ViewTradeDetailsCommand = new DelegateCommand<TradeAggregate>(ExecuteViewTradeDetails, entry => entry != null);
            ViewMarketDetailsCommand =
                new DelegateCommand<TradeAggregate>(
                    item => _eventAggregator.PublishOnUIThread(new ViewMarketDetailsEventArgs(item.InvType)), entry => entry != null);
            ViewPeriodCommand = new DelegateCommand(ViewPeriod);
            ViewOrderCommand = new DelegateCommand<TradeAggregate>(ExecuteViewOrder, CanViewOrder);
            AddToOrdersCommand = new DelegateCommand<ICollection<object>>(ExecuteAddToOrders, CanAddToOrders);

            PeriodSelectorStart = DateTime.UtcNow.AddMonths(-1);
            PeriodSelectorEnd = DateTime.UtcNow;
        }

        private bool CanAddToOrders(ICollection<object> arg) {
            if (arg == null || !arg.Any())
                return false;
            List<TradeAggregate> items = arg.Select(item => (TradeAggregate)item).ToList();
            return items.All(item => item.Order == null);
        }

        private void ExecuteAddToOrders(ICollection<object> obj) {
            List<InvType> items = obj.Select(item => ((TradeAggregate)item).InvType).ToList();
            _eventAggregator.PublishOnUIThread(new AddToOrdersEventArgs(items));
        }

        public DateTime ActualViewStart { get; private set; }

        public DateTime ActualViewEnd { get; private set; }

        public DateTime PeriodSelectorStart { get; set; }

        public DateTime PeriodSelectorEnd { get; set; }

        public ICommand ViewTradeDetailsCommand { get; private set; }

        public ICommand ViewMarketDetailsCommand { get; private set; }

        public ICommand ViewOrderCommand { get; private set; }

        public ICommand ViewPeriodCommand { get; private set; }

        public ICommand AddToOrdersCommand { get; private set; }



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

        public BindableCollection<TradeAggregate> Items { get; private set; }


        private bool CanViewOrder(TradeAggregate entry) {
            return entry != null && entry.Order != null;
        }

        private void ExecuteViewOrder(TradeAggregate entry) {
            _eventAggregator.PublishOnUIThread(new ViewOrderEventArgs(entry.InvType));
        }

        private void ExecuteViewTradeDetails(TradeAggregate entry) {
                _eventAggregator.PublishOnUIThread(new ViewTradeDetailsEventArgs(entry.InvType));
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
            await Task.Run(() => analyze(ActualViewStart, ActualViewEnd)).ConfigureAwait(false);
        }

        private void analyze(DateTime start, DateTime end) {
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Analyzing..."));

            Items.IsNotifying = false;
            Items.Clear();
            IQueryable<IGrouping<int, Transaction>> transactionGroups = _dataService.Db.Transactions.AsNoTracking().Where(
                t =>
                    t.TransactionDate > start.Date && t.TransactionDate <= end.Date &&
                    t.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id)
                .GroupBy(t => t.TypeId);
            ILookup<int, Order> orders =
                _dataService.Db.Orders.Where(order => order.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id)
                    .ToLookup(order => order.TypeId);
            foreach (var transactionCollection in transactionGroups) {
                Items.Add(new TradeAggregate(transactionCollection.First().InvType, transactionCollection.ToList(),
                    orders[transactionCollection.First().TypeId].SingleOrDefault()));
            }
            Items.IsNotifying = true;
            Items.Refresh();
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Analysis complete"));
        }
    }
}