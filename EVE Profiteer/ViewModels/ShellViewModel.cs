using System.Collections.Generic;
using System.Diagnostics;
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
using eZet.EveProfiteer.ViewModels.Dialogs;
using eZet.EveProfiteer.ViewModels.Tabs;

namespace eZet.EveProfiteer.ViewModels {
    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive, IShell, IHandle<ViewTradeDetailsEventArgs>,
        IHandle<ViewMarketDetailsEventArgs>, IHandle<ViewOrderEventArgs>, IHandle<AddToOrdersEventArgs>,
        IHandle<StatusChangedEventArgs> {
        private readonly TransactionService TransactionService;
        private readonly EveProfiteerDataService _dataService;
        private readonly AssetService _assetService;
        private readonly EveApiService _eveApiService;
        private readonly IEventAggregator _eventAggregator;
        private readonly KeyManagementService _keyManagementService;
        private readonly IWindowManager _windowManager;
        private readonly TraceSource _trace = new TraceSource("Main");
        private string _statusMessage;

        public ShellViewModel(IWindowManager windowManager, IEventAggregator eventAggregator,
            KeyManagementService keyManagementService,
            TransactionService transactionService, EveApiService eveApiService, EveProfiteerDataService dataService, AssetService assetService) {
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _keyManagementService = keyManagementService;
            TransactionService = transactionService;
            _eveApiService = eveApiService;
            _dataService = dataService;
            _assetService = assetService;

            DisplayName = "EVE Profiteer";
            StatusMessage = "Ready";

            ActiveKey = _keyManagementService.AllApiKeys().FirstOrDefault();
            _eventAggregator.Subscribe(this);
            if (ActiveKey != null)
                ActiveKeyEntity = ActiveKey.ApiKeyEntities.Single(f => f.EntityId == 977615922);

            ManageKeysCommand = new DelegateCommand(ManageKeys);
            UpdateTransactionsCommand = new DelegateCommand(UpdateTransactions);
            UpdateItemCostsCommand = new DelegateCommand(UpdateItemCosts);
            UpdateAssetsCommand = new DelegateCommand(ExecuteUpdateAssets);


            TradeSummary = IoC.Get<TradeSummaryViewModel>();
            TradeAnalyzer = IoC.Get<TradeAnalyzerViewModel>();
            TradeDetails = IoC.Get<TradeDetailsViewModel>();
            MarketBrowser = IoC.Get<MarketBrowserViewModel>();
            MarketAnalyzer = IoC.Get<MarketAnalyzerViewModel>();
            OrderEditor = IoC.Get<OrderEditorViewModel>();
            Assets = IoC.Get<AssetsViewModel>();
            ProductionModel = IoC.Get<ProductionViewModel>();
            SelectKey();
            InitAsync();
        }

        public AssetsViewModel Assets { get; set; }

        public TradeSummaryViewModel TradeSummary { get; set; }

        public TradeAnalyzerViewModel TradeAnalyzer { get; set; }

        public TradeDetailsViewModel TradeDetails { get; set; }

        public MarketBrowserViewModel MarketBrowser { get; set; }

        public MarketAnalyzerViewModel MarketAnalyzer { get; set; }

        public OrderEditorViewModel OrderEditor { get; set; }
        public ProductionViewModel ProductionModel { get; set; }

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

        public ICommand UpdateAssetsCommand { get; private set; }

        public ICommand ManageKeysCommand { get; private set; }

        public ICommand UpdateTransactionsCommand { get; private set; }

        public static ApiKey ActiveKey { get; private set; }

        public static ApiKeyEntity ActiveKeyEntity { get; private set; }

        public void Handle(AddToOrdersEventArgs message) {
            //ActivateItem(Items.Single(item => item.GetType() == typeof (OrderEditorViewModel)));
        }

        public void Handle(StatusChangedEventArgs message) {
            _trace.TraceEvent(TraceEventType.Information, 0, "StatusChangedEventArgs: {0}", message.Status);
            StatusMessage = message.Status;
        }

        public void Handle(ViewMarketDetailsEventArgs message) {
            _trace.TraceEvent(TraceEventType.Verbose, 0, "ViewMarketDetailsEventArgs");
            ActivateItem(MarketBrowser);
        }

        public void Handle(ViewOrderEventArgs message) {
            _trace.TraceEvent(TraceEventType.Verbose, 0, "ViewOrdersEventArgs");
            ActivateItem(OrderEditor);
        }

        public void Handle(ViewTradeDetailsEventArgs message) {
            _trace.TraceEvent(TraceEventType.Verbose, 0, "ViewTradeDetailsEventArgs");
            ActivateItem(TradeDetails);
        }

        public async void SelectKey() {
            Items.Clear();
            ApplicationHelper.ActiveKeyEntity = ActiveKeyEntity;
            ApplicationHelper.ActiveKey = ActiveKey;
            //ApplicationHelper.ActiveKey = ActiveKey;


            //var transactions = IoC.Get<TransactionsViewModel>();
            //Items.Add(transactions);
            //var journal = IoC.Get<JournalViewModel>();
            //Items.Add(journal);

            Items.Add(TradeSummary);
            Items.Add(TradeAnalyzer);
            Items.Add(TradeDetails);
            Items.Add(MarketBrowser);
            Items.Add(MarketAnalyzer);
            Items.Add(OrderEditor);
            Items.Add(Assets);


            //Items.Add(IoC.Get<TradeDetailsViewModel>());
            //Items.Add(IoC.Get<ProfitViewModel>());

            if (ActiveKey != null) {
                //transactions.Initialize(ActiveKey, ActiveKeyEntity);
                // journal.Initialize(ActiveKey, ActiveKeyEntity);
            }
        }

        public async void InitAsync() {
            _trace.TraceEvent(TraceEventType.Verbose, 0, "TradeSummary.InitAsync");
            await TradeSummary.InitAsync();
            _trace.TraceEvent(TraceEventType.Verbose, 0, "TradeDetails.InitAsync");
            await TradeDetails.InitAsync();
            _trace.TraceEvent(TraceEventType.Verbose, 0, "MarketBrowser.InitAsync");
            await MarketBrowser.InitAsync();
            _trace.TraceEvent(TraceEventType.Verbose, 0, "MarketAnalyzer.InitAsync");
            await MarketAnalyzer.InitAsync();
            _trace.TraceEvent(TraceEventType.Verbose, 0, "OrderEditor.InitAsync");
            await OrderEditor.InitAsync();
            _trace.TraceEvent(TraceEventType.Verbose, 0, "Assets.InitAsync");
            await Assets.InitAsync();
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Ready"));
        }

        public void ManageKeys() {
            _windowManager.ShowDialog(IoC.Get<ManageKeysViewModel>());
        }

        public async void ExecuteUpdateAssets() {
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Fetching assets..."));
            await _assetService.UpdateAssets();
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Assets updated"));
            await _eventAggregator.PublishOnUIThreadAsync(new AssetsUpdatedEvent()).ConfigureAwait(false);
        }

        public async void UpdateTransactions() {
            _trace.TraceEvent(TraceEventType.Start, 0, "StartUpdateTransactions");

            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Fetching new transactions..."));
            long latest = 0;
            latest = TransactionService.GetLatestId(ActiveKeyEntity);
            _trace.TraceEvent(TraceEventType.Verbose, 0, "Latest transaction ID: " + latest);

            IEnumerable<Transaction> list =
                await Task.Run(() => _eveApiService.GetNewTransactions(ActiveKey, ActiveKeyEntity, latest));
            IList<Transaction> transactions = list as IList<Transaction> ?? list.ToList();
            _trace.TraceEvent(TraceEventType.Verbose, 0, "Fetched transactions: " + transactions.Count);
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Processing transactions..."));
            await TransactionService.ProcessTransactions(transactions);
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Transaction update complete"));
            _trace.TraceEvent(TraceEventType.Stop, 0, "CompleteUpdateTransactions");
        }

        public async void UpdateItemCosts() {
            //await TransactionService.ProcessInventory(TransactionService.Db.Transactions.ToList());
            //_eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Saved"));
        }
    }
}