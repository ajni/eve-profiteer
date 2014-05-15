using System.Linq;
using System.Windows.Controls.Primitives;
using Caliburn.Micro;
using eZet.EveProfiteer.Framework;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels {
    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive, IShell {
        private readonly KeyManagementService _keyManagementService;
        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _eventAggregator;

        public ShellViewModel(IWindowManager windowManager, IEventAggregator eventAggregator, KeyManagementService keyManagementService) {
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _keyManagementService = keyManagementService;
            ActiveKey = keyManagementService.AllApiKeys().FirstOrDefault();
            if (ActiveKey != null) ActiveKeyEntity = ActiveKey.ApiKeyEntities.Single(f => f.EntityId == 977615922);
            SelectKey();
        }

        public ApiKey ActiveKey { get; private set; }

        public ApiKeyEntity ActiveKeyEntity { get; private set; }

        public void SelectKey() {
            Items.Clear();

            Items.Add(IoC.Get<OverviewViewModel>());
            var transactions = IoC.Get<TransactionsViewModel>();
            Items.Add(transactions);

            var journal = IoC.Get<JournalViewModel>();
            Items.Add(journal);

            Items.Add(IoC.Get<MarketAnalyzerViewModel>());

            Items.Add(IoC.Get<OrderEditorViewModel>());
            Items.Add(IoC.Get<TradeAnalyzerViewModel>());

            //Items.Add(IoC.Get<ItemDetailsViewModel>());
            //Items.Add(IoC.Get<ProfitViewModel>());

            if (ActiveKey != null) {
                 transactions.Initialize(ActiveKey, ActiveKeyEntity);
                // journal.Initialize(ActiveKey, ActiveKeyEntity);
            }
        }

        public void ManageKeys() {
            _windowManager.ShowDialog(IoC.Get<ManageKeysViewModel>());
        }
    }
}