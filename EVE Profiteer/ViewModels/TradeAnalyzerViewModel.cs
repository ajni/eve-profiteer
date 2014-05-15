using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels {
    public class TradeAnalyzerViewModel : Screen {

        public enum ViewPeriodEnum {
            LastDay,
            LastWeek,
            LastMonth,
            All,
            Custom
        }

        private readonly IWindowManager _windowMananger;
        private readonly IEventAggregator _eventAggregator;
        private readonly TransactionService _transactionService;
        private ViewPeriodEnum _selectedViewPeriod;

        public DateTime ViewStart { get; set; }

        public DateTime ViewEnd { get; set; }

        public IEnumerable<ViewPeriodEnum> ViewPeriodValues {
            get { return Enum.GetValues(typeof (ViewPeriodEnum)).Cast<ViewPeriodEnum>(); }
        }

        public ViewPeriodEnum SelectedViewPeriod {
            get { return _selectedViewPeriod; }
            set { _selectedViewPeriod = value; NotifyOfPropertyChange(() => SelectedViewPeriod); }
        }


        public BindableCollection<TradeAnalyzerItem> Items { get; private set; }

        public TradeAnalyzerViewModel(IWindowManager windowMananger, IEventAggregator eventAggregator, TransactionService transactionService) {
            _windowMananger = windowMananger;
            _eventAggregator = eventAggregator;
            _transactionService = transactionService;
            DisplayName = "Trade Analyzer";
            Items = new BindableCollection<TradeAnalyzerItem>();
        }

        public async Task ViewPeriod() {
            Items.Clear();
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
                case ViewPeriodEnum.LastMonth:
                    ViewStart = DateTime.UtcNow.AddDays(-7);
                    break;
            }
            await Task.Run(() => load());
        }

        private void load() {
            var transactions = _transactionService.Transactions().Where(t => t.TransactionDate >= ViewStart && t.TransactionDate <= ViewEnd).GroupBy(t => t.TypeId);
            var items = new List<TradeAnalyzerItem>();
            foreach (var transactionCollection in transactions.Select(x => x.ToList())) {
                items.Add(new TradeAnalyzerItem(transactionCollection.First().TypeId, transactionCollection.First().TypeName, transactionCollection));
            }
            Items.AddRange(items);
        }
    }
}
