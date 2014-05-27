using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using DevExpress.Xpf.Mvvm;
using eZet.EveProfiteer.Events;
using eZet.EveProfiteer.Framework;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;
using eZet.EveProfiteer.Util;

namespace eZet.EveProfiteer.ViewModels {
    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive, IShell, IHandle<ViewTradeDetailsEventArgs>,
        IHandle<ViewMarketDetailsEventArgs>, IHandle<ViewOrderEventArgs>, IHandle<AddToOrdersEventArgs>,
        IHandle<StatusChangedEventArgs> {
        private readonly EveApiService _eveApiService;
        private readonly EveProfiteerDataService _dataService;
        private readonly IEventAggregator _eventAggregator;
        private readonly KeyManagementService _keyManagementService;
        private readonly TransactionService TransactionService;
        private readonly IWindowManager _windowManager;
        private string _statusMessage;

        public ShellViewModel(IWindowManager windowManager, IEventAggregator eventAggregator,
            KeyManagementService keyManagementService,
            TransactionService transactionService, EveApiService eveApiService, EveProfiteerDataService dataService) {
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _keyManagementService = keyManagementService;
            TransactionService = transactionService;
            _eveApiService = eveApiService;
            _dataService = dataService;
            ActiveKey = _keyManagementService.AllApiKeys().FirstOrDefault();
            _eventAggregator.Subscribe(this);
            if (ActiveKey != null)
                ActiveKeyEntity = ActiveKey.ApiKeyEntities.Single(f => f.EntityId == 977615922);

            ManageKeysCommand = new DelegateCommand(ManageKeys);
            UpdateTransactionsCommand = new DelegateCommand(UpdateTransactions);
            UpdateItemCostsCommand = new DelegateCommand(UpdateItemCosts);
            DisplayName = "EVE Profiteer";
            StatusMessage = "Ready";
            SelectKey();
        }

        public string StatusMessage {
            get { return _statusMessage; }
            set {
                if (value == _statusMessage) return;
                _statusMessage = value;
                NotifyOfPropertyChange(() => StatusMessage);
            }
        }

        public string CurrentEntityName {
            get { return ApplicationHelper.ActiveKeyEntity.Name; }
        }

        public ICommand UpdateItemCostsCommand { get; private set; }

        public ICommand ManageKeysCommand { get; private set; }

        public ICommand UpdateTransactionsCommand { get; private set; }

        public static ApiKey ActiveKey { get; private set; }

        public static ApiKeyEntity ActiveKeyEntity { get; private set; }

        public void Handle(AddToOrdersEventArgs message) {
            ActivateItem(Items.Single(item => item.GetType() == typeof (OrderEditorViewModel)));
        }

        public void Handle(StatusChangedEventArgs message) {
            StatusMessage = message.Status;
        }

        public void Handle(ViewMarketDetailsEventArgs message) {
            ActivateItem(Items.Single(item => item.GetType() == typeof (MarketBrowserViewModel)));
        }

        public void Handle(ViewOrderEventArgs message) {
            ActivateItem(Items.Single(item => item.GetType() == typeof (OrderEditorViewModel)));
        }

        public void Handle(ViewTradeDetailsEventArgs message) {
            ActivateItem(Items.Single(item => item.GetType() == typeof (TradeDetailsViewModel)));
        }

        public void SelectKey() {
            Items.Clear();
            ApplicationHelper.ActiveKeyEntity = ActiveKeyEntity;
            //ApplicationHelper.ActiveKey = ActiveKey;

            Items.Add(IoC.Get<OverviewViewModel>());

            //var transactions = IoC.Get<TransactionsViewModel>();
            //Items.Add(transactions);
            //var journal = IoC.Get<JournalViewModel>();
            //Items.Add(journal);

            Items.Add(IoC.Get<MarketAnalyzerViewModel>());
            Items.Add(IoC.Get<OrderEditorViewModel>());
            Items.Add(IoC.Get<TradeAnalyzerViewModel>());
            Items.Add(IoC.Get<TradeDetailsViewModel>());
            Items.Add(IoC.Get<MarketBrowserViewModel>());

            //Items.Add(IoC.Get<TradeDetailsViewModel>());
            //Items.Add(IoC.Get<ProfitViewModel>());

            if (ActiveKey != null) {
                //transactions.Initialize(ActiveKey, ActiveKeyEntity);
                // journal.Initialize(ActiveKey, ActiveKeyEntity);

            }
        }
        public void ManageKeys() {
            _windowManager.ShowDialog(IoC.Get<ManageKeysViewModel>());
        }

        public async void UpdateTransactions() {
            _eventAggregator.Publish(new StatusChangedEventArgs("Fetching new transactions..."));
            long latest = 0;
            latest = TransactionService.GetLatestId(ActiveKeyEntity);
            IEnumerable<Transaction> list = await Task.Run(() => _eveApiService.GetNewTransactions(ActiveKey, ActiveKeyEntity, latest));
            IList<Transaction> transactions = list as IList<Transaction> ?? list.ToList();
            _eventAggregator.Publish(new StatusChangedEventArgs("Processing transactions..."));
            await TransactionService.ProcessTransactions(transactions);
            _eventAggregator.Publish(new StatusChangedEventArgs("Transaction update complete"));

        }

        public async void UpdateItemCosts() {
            await TransactionService.ProcessInventory(TransactionService.Db.Transactions.ToList());
            _eventAggregator.Publish(new StatusChangedEventArgs("Saved"));

        }
    }
}