using System.Linq;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels {
    public class ItemDetailsViewModel : Screen {
        private readonly TradeAnalyzerService _tradeAnalyzerService;

        public ItemDetailsData ItemData { get; set; }

        public ItemDetailsViewModel(TradeAnalyzerService tradeAnalyzerService) {
            _tradeAnalyzerService = tradeAnalyzerService;
            DisplayName = "Item Details";
            var transactions = tradeAnalyzerService.Transactions().Where(f => f.TypeId == 6863).ToList();
            ItemData = new ItemDetailsData(transactions.First().TypeId, transactions.First().TypeName, transactions);
            ItemData.Analyze();
        }

    }
}
