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

namespace eZet.EveProfiteer.ViewModels {
    public class TradeAnalyzerViewModel : Screen {
        public enum ViewPeriodEnum {
            LastDay,
            LastWeek,
            LastTwoWeeks,
            LastMonth,
            All,
            CustomDaySpan,
            CustomPeriod
        }

        private readonly AnalyzerService _analyzerService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IWindowManager _windowMananger;
        private ViewPeriodEnum _selectedViewPeriod;

        public TradeAnalyzerViewModel(IWindowManager windowMananger, IEventAggregator eventAggregator,
            AnalyzerService analyzerService) {
            _windowMananger = windowMananger;
            _eventAggregator = eventAggregator;
            _analyzerService = analyzerService;
            DisplayName = "Trade Analyzer";
            Items = new BindableCollection<TradeAnalyzerItem>();
            ViewTradeDetailsCommand = new DelegateCommand<TradeAnalyzerItem>(ViewTradeDetails, CanViewTradeDetails);
        }

        private bool CanViewTradeDetails(TradeAnalyzerItem tradeAnalyzerItem) {
            if (tradeAnalyzerItem != null && tradeAnalyzerItem.Order != null)
                return true;
            return false;
        }

        private void ViewTradeDetails(TradeAnalyzerItem tradeAnalyzerItem) {
            if (tradeAnalyzerItem != null) {
                _eventAggregator.Publish(new ViewTradeDetailsEventArgs(tradeAnalyzerItem.TypeId));
            }
        }

        public DateTime ViewStart { get; set; }

        public DateTime ViewEnd { get; set; }

        public ICommand ViewTradeDetailsCommand { get; private set; }

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

        public BindableCollection<TradeAnalyzerItem> Items { get; private set; }


        public int CustomDaySpan { get; set; }

        public async Task ViewPeriod() {
            ViewEnd = DateTime.UtcNow;
            switch (SelectedViewPeriod) {
                case ViewPeriodEnum.All:
                    ViewStart = DateTime.MinValue;
                    break;
                case ViewPeriodEnum.LastDay:
                    ViewStart = DateTime.UtcNow.AddDays(-1);
                    break;
                case ViewPeriodEnum.LastWeek:
                    ViewStart = DateTime.UtcNow.AddDays(-7);
                    break;
                case ViewPeriodEnum.LastTwoWeeks:
                    ViewStart = DateTime.UtcNow.AddDays(-14);
                    break;
                case ViewPeriodEnum.LastMonth:
                    ViewStart = DateTime.UtcNow.AddMonths(-7);
                    break;
                case ViewPeriodEnum.CustomDaySpan:
                    ViewStart = DateTime.UtcNow.AddDays(-CustomDaySpan);
                    break;
            }
            await Task.Run(() => load());
        }

        private void load() {
            List<Order> orders = _analyzerService.Orders().ToList();
            ILookup<int, Order> orderLookup = orders.ToLookup(order => order.TypeId);
            IQueryable<IGrouping<int, Transaction>> transactions =
                _analyzerService.Transactions()
                    .Where(t => t.TransactionDate >= ViewStart && t.TransactionDate <= ViewEnd)
                    .GroupBy(t => t.TypeId);
            var items = new List<TradeAnalyzerItem>();
            foreach (var transactionCollection in transactions.Select(x => x.ToList())) {
                int typeId = transactionCollection.First().TypeId;
                items.Add(new TradeAnalyzerItem(typeId, transactionCollection.First().TypeName, transactionCollection,
                    orderLookup[typeId].SingleOrDefault()));
            }
            Items.Clear();
            Items.AddRange(items);
        }
    }
}