using Caliburn.Micro;
using eZet.EveProfiteer.Framework;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.ViewModels {
    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive, IShell {

        private readonly IWindowManager windowManager;

        public ShellViewModel(IWindowManager windowManager) {
            this.windowManager = windowManager;
            Items.Add(IoC.Get<OverviewViewModel>());
            var key = new ApiKey();
            key.ApiKeyId = 3053778;
            key.VCode = "Hu3uslqNc3HDP8XmMMt1Cgb56TpPqqnF2tXssniROFkIMEDLztLPD8ktx6q5WVC2";
            var entity = new ApiKeyEntity();
            entity.Id = 1;
            entity.EntityId = 977615922;

            var transactions = IoC.Get<TransactionsViewModel>();
            transactions.Initialize(key, entity);
            Items.Add(transactions);

            var journal = IoC.Get<JournalViewModel>();
            journal.Initialize(key, entity);
            Items.Add(journal);

            Items.Add(IoC.Get<MarketAnalyzerViewModel>());
        }

        public void ManageKeys() {
            windowManager.ShowDialog(IoC.Get<ManageKeysViewModel>());
        }



    }
}
