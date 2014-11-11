using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using DevExpress.Mvvm;
using eZet.EveProfiteer.Events;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;
using eZet.EveProfiteer.Ui.Events;

namespace eZet.EveProfiteer.ViewModels.Modules {
    public sealed class TradeAnalyzerViewModel : ModuleViewModel {
        public enum ViewPeriodEnum {
            Today,
            Yesterday,
            Week,
            Month,
            All,
            Since,
            Period
        }

        private readonly TradeAnalyzerService _tradeAnalyzerService;
        private readonly IEventAggregator _eventAggregator;
        private ViewPeriodEnum selectedViewPeriod;
        private BindableCollection<TransactionAggregate> items;

        public TradeAnalyzerViewModel(IEventAggregator eventAggregator, TradeAnalyzerService tradeAnalyzerService) {
            _eventAggregator = eventAggregator;
            _tradeAnalyzerService = tradeAnalyzerService;
            ViewTradeDetailsCommand = new DelegateCommand<TransactionAggregate>(ExecuteViewTradeDetails,
                entry => entry != null);
            ViewMarketDetailsCommand =
                new DelegateCommand<TransactionAggregate>(
                    item => _eventAggregator.PublishOnUIThread(new ViewMarketBrowserEvent(item.InvType)),
                    entry => entry != null);
            ViewPeriodCommand = new DelegateCommand(ViewPeriod);
            ViewOrderCommand = new DelegateCommand<TransactionAggregate>(ExecuteViewOrder, CanViewOrder);
            AddToOrdersCommand = new DelegateCommand<ICollection<object>>(ExecuteAddToOrders, CanAddToOrders);
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

        public ICommand AddToOrdersCommand { get; private set; }


        public IEnumerable<ViewPeriodEnum> ViewPeriodValues {
            get { return Enum.GetValues(typeof(ViewPeriodEnum)).Cast<ViewPeriodEnum>(); }
        }

        public ViewPeriodEnum SelectedViewPeriod {
            get { return selectedViewPeriod; }
            set {
                if (value == selectedViewPeriod) return;
                selectedViewPeriod = value;
                NotifyOfPropertyChange();
            }
        }

        public BindableCollection<TransactionAggregate> Items {
            get { return items; }
            private set {
                if (Equals(value, items)) return;
                items = value;
                NotifyOfPropertyChange();
            }
        }

        protected override Task OnDeactivate(bool close) {
            if (close) {
                Items = null;
            }
            return base.OnDeactivate(close);
        }

        private bool CanAddToOrders(ICollection<object> arg) {
            if (arg == null || !arg.Any())
                return false;
            List<TransactionAggregate> items = arg.Select(item => (TransactionAggregate)item).ToList();
            return items.All(item => item.Order == null);
        }

        private void ExecuteAddToOrders(ICollection<object> obj) {
            List<InvType> items = obj.Select(item => ((TransactionAggregate)item).InvType).ToList();
            _eventAggregator.PublishOnUIThread(new AddOrdersEvent(items));
        }

        private bool CanViewOrder(TransactionAggregate entry) {
            return entry != null && entry.Order != null;
        }

        private void ExecuteViewOrder(TransactionAggregate entry) {
            _eventAggregator.PublishOnUIThread(new ViewOrderEvent(entry.InvType));
        }

        private void ExecuteViewTradeDetails(TransactionAggregate entry) {
            _eventAggregator.PublishOnUIThread(new ViewTransactionDetailsEvent(entry.InvType));
        }

        public async void ViewPeriod() {
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Analyzing..."));

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
            await analyze(ActualViewStart, ActualViewEnd).ConfigureAwait(false);
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Analysis complete"));

        }

        private async Task analyze(DateTime start, DateTime end) {
            var items = new List<TransactionAggregate>();
            var transactionGroups = (await _tradeAnalyzerService.GetTransactions(start, end).ConfigureAwait(false)).GroupBy(t => t.InvType);
            ILookup<int, Order> orders = (await _tradeAnalyzerService.GetOrders()).ToLookup(order => order.TypeId);
            foreach (var transactionCollection in transactionGroups) {
                items.Add(new TransactionAggregate(transactionCollection.Key, transactionCollection.ToList(),
                    orders[transactionCollection.First().TypeId].SingleOrDefault()));
            }
            Items = new BindableCollection<TransactionAggregate>(items);
        }

    }
}