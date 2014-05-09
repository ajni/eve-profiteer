using System.Linq;
using Caliburn.Micro;
using eZet.EveProfiteer.Framework;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels {
    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive, IShell {

        private readonly IWindowManager _windowManager;
        private readonly KeyManagementService _keyManagementService;

        public ApiKey ActiveKey { get; private set; }

        public ApiKeyEntity ActiveKeyEntity { get; private set; }

        public ShellViewModel(IWindowManager windowManager, KeyManagementService keyManagementService) {
            _windowManager = windowManager;
            _keyManagementService = keyManagementService;
            ActiveKey = keyManagementService.AllApiKeys().FirstOrDefault();
            if (ActiveKey != null) ActiveKeyEntity = ActiveKey.Entities.Single(f => f.EntityId == 977615922);
            SelectKey();
        }

        public void SelectKey() {
            Items.Clear();

            Items.Add(IoC.Get<OverviewViewModel>());
            var transactions = IoC.Get<TransactionsViewModel>();
            Items.Add(transactions);

            var journal = IoC.Get<JournalViewModel>();
            Items.Add(journal);

            Items.Add(IoC.Get<MarketAnalyzerViewModel>());
            Items.Add(IoC.Get<OrderEditorViewModel>());
            //Items.Add(IoC.Get<ItemDetailsViewModel>());
            //Items.Add(IoC.Get<ProfitViewModel>());

            if (ActiveKey != null) {
              // transactions.Initialize(ActiveKey, ActiveKeyEntity);
              // journal.Initialize(ActiveKey, ActiveKeyEntity);
            }

        }

        public void ManageKeys() {
            _windowManager.ShowDialog(IoC.Get<ManageKeysViewModel>());
        }



    }
}
