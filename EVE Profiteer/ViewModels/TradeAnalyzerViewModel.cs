using System.Linq;
using Caliburn.Micro;
using DevExpress.Data.WcfLinq.Helpers;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels {
    public class TradeAnalyzerViewModel : Screen {
        private readonly IWindowManager _windowMananger;
        private readonly IEventAggregator _eventAggregator;
        private readonly TransactionService _transactionService;

        public BindableCollection<TradeAnalyzerItem> Items { get; private set; }

        public TradeAnalyzerViewModel(IWindowManager windowMananger, IEventAggregator eventAggregator, TransactionService transactionService) {
            _windowMananger = windowMananger;
            _eventAggregator = eventAggregator;
            _transactionService = transactionService;
            DisplayName = "Trade Analyzer";Items = new BindableCollection<TradeAnalyzerItem>();

        }

        protected override void OnInitialize() {
            var items = _transactionService.All().GroupBy(t => t.TypeId);
            foreach (var transactions in items.Select(x => x.ToList())) {
                Items.Add(new TradeAnalyzerItem(transactions.First().TypeId, transactions.First().TypeName, transactions));
            }


        }
    }
}
