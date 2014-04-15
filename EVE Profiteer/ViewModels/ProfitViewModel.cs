using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using eZet.EveLib.Modules.Models;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels {
    public class ProfitViewModel : Screen {
        public TransactionService TransactionService { get; set; }

        public ICollection<ItemProfit> Items { get; set; }

        public ProfitViewModel(TransactionService transactionService) {
            Items = new List<ItemProfit>();
            DisplayName = "Item Profit";
            TransactionService = transactionService;
            var items = TransactionService.All().Where(f => f.ApiKeyEntity.EntityId != -1).GroupBy(f => f.TypeId);
            foreach (var item in items) {
                Items.Add(new ItemProfit(new Item(), item.Where(f => f.TransactionType == OrderType.Buy), item.Where(f => f.TransactionType == OrderType.Sell)));
            }
        }
    }
}
