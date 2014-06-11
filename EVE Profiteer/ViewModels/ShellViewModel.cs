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
        private readonly IEventAggregator _eventAggregator;
        private readonly KeyManagementService _keyManagementService;
        private readonly ShellService _shellService;
        private readonly TraceSource _trace = new TraceSource("EveProfiteer", SourceLevels.All);
        private readonly IWindowManager _windowManager;
        private string _statusMessage;

        public ShellViewModel(IWindowManager windowManager, IEventAggregator eventAggregator,
            KeyManagementService keyManagementService, ShellService shellService) {
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _keyManagementService = keyManagementService;

            _shellService = shellService;

            DisplayName = "EVE Profiteer";
            AllowStatusChange = true;
            StatusMessage = "Ready";


            ActiveKey = _keyManagementService.AllApiKeys().FirstOrDefault();
            _eventAggregator.Subscribe(this);
            if (ActiveKey != null)
                ActiveKeyEntity = ActiveKey.ApiKeyEntities.Single(f => f.EntityId == 977615922);

            ManageKeysCommand = new DelegateCommand(ManageKeys);
            UpdateTransactionsCommand = new DelegateCommand(ExecuteUpdateTransactions);
            RecalculateInventoryCommand = new DelegateCommand(ExecuteRecalculateInventory);
            UpdateAssetsCommand = new DelegateCommand(ExecuteUpdateAssets);
            UpdateJournalCommand = new DelegateCommand(ExecuteUpdateJournal);
            UpdateApiCommand = new DelegateCommand(ExecuteUpdateApi);
            UpdateRefTypesCommand = new DelegateCommand(ExecuteUpdateRefTypes);


            TradeSummary = IoC.Get<TradeSummaryViewModel>();
            TradeAnalyzer = IoC.Get<TradeAnalyzerViewModel>();
            TradeDetails = IoC.Get<TradeDetailsViewModel>();
            MarketBrowser = IoC.Get<MarketBrowserViewModel>();
            MarketAnalyzer = IoC.Get<MarketAnalyzerViewModel>();
            OrderEditor = IoC.Get<OrderEditorViewModel>();
            Assets = IoC.Get<AssetsViewModel>();
            ProductionModel = IoC.Get<ProductionViewModel>();
            TransactionsViewModel = IoC.Get<TransactionsViewModel>();
            JournalViewModel = IoC.Get<JournalViewModel>();
            SelectKey();
            Task.Run(() => InitAsync());
        }

        public AssetsViewModel Assets { get; set; }

        public TradeSummaryViewModel TradeSummary { get; set; }

        public TradeAnalyzerViewModel TradeAnalyzer { get; set; }

        public TradeDetailsViewModel TradeDetails { get; set; }

        public MarketBrowserViewModel MarketBrowser { get; set; }

        public MarketAnalyzerViewModel MarketAnalyzer { get; set; }

        public OrderEditorViewModel OrderEditor { get; set; }
        public ProductionViewModel ProductionModel { get; set; }

        public TransactionsViewModel TransactionsViewModel { get; set; }

        public JournalViewModel JournalViewModel { get; set; }

        public string StatusMessage {
            get { return _statusMessage; }
            set {
                if (value == _statusMessage) return;
                _statusMessage = value;
                NotifyOfPropertyChange(() => StatusMessage);
            }
        }

        public bool AllowStatusChange { get; set; }

        public string CurrentEntityName {
            get { return ApplicationHelper.ActiveKeyEntity.Name; }
        }

        public ICommand RecalculateInventoryCommand { get; private set; }

        public ICommand UpdateAssetsCommand { get; private set; }

        public ICommand ManageKeysCommand { get; private set; }

        public ICommand UpdateTransactionsCommand { get; private set; }

        public ICommand UpdateJournalCommand { get; private set; }

        public ICommand UpdateApiCommand { get; private set; }

        public static ApiKey ActiveKey { get; private set; }

        public static ApiKeyEntity ActiveKeyEntity { get; private set; }

        public ICommand UpdateRefTypesCommand { get; private set; }

        public void Handle(AddToOrdersEventArgs message) {
            //ActivateItem(Items.Single(item => item.GetType() == typeof (OrderEditorViewModel)));
        }

        public void Handle(StatusChangedEventArgs message) {
            _trace.TraceEvent(TraceEventType.Information, 0, "StatusChangedEventArgs: {0}", message.Status);
            if (AllowStatusChange) {
                StatusMessage = message.Status;
            }
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
            Items.Add(ProductionModel);
            Items.Add(TransactionsViewModel);
            Items.Add(JournalViewModel);


            //Items.Add(IoC.Get<TradeDetailsViewModel>());
            //Items.Add(IoC.Get<ProfitViewModel>());

            if (ActiveKey != null) {
                //transactions.Initialize(ActiveKey, ActiveKeyEntity);
                // journal.Initialize(ActiveKey, ActiveKeyEntity);
            }
        }

        public async void InitAsync() {
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Initializing..."));


            _trace.TraceEvent(TraceEventType.Verbose, 0, "TradeSummaryViewModel.InitAsync");
            await TradeSummary.InitAsync();
            _trace.TraceEvent(TraceEventType.Verbose, 0, "TradeDetailsViewModel.InitAsync");
            await TradeDetails.InitAsync();
            _trace.TraceEvent(TraceEventType.Verbose, 0, "MarketBrowserViewModel.InitAsync");
            await MarketBrowser.InitAsync();
            _trace.TraceEvent(TraceEventType.Verbose, 0, "MarketAnalyzerViewModel.InitAsync");
            await MarketAnalyzer.InitAsync();
            _trace.TraceEvent(TraceEventType.Verbose, 0, "OrderEditorViewModel.InitAsync");
            await OrderEditor.InitAsync();
            _trace.TraceEvent(TraceEventType.Verbose, 0, "AssetsViewModel.InitAsync");
            await Assets.InitAsync();
            _trace.TraceEvent(TraceEventType.Verbose, 0, "ProductionViewModel.InitAsync");
            await ProductionModel.InitAsync();
            _trace.TraceEvent(TraceEventType.Verbose, 0, "TransactionsViewModel.InitAsync");
            await TransactionsViewModel.InitAsync();
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Ready"));
        }

        public void ManageKeys() {
            _windowManager.ShowDialog(IoC.Get<ManageKeysViewModel>());
        }

        public async void ExecuteUpdateApi() {
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Updating Assets..."));
            await _shellService.UpdateAssetsAsync().ConfigureAwait(false);
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Updating Transactions..."));
            await _shellService.UpdateTransactionsAsync().ConfigureAwait(false);
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Updating Journal..."));
            await _shellService.UpdateJournalAsync().ConfigureAwait(false);
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("API Update Complete"));
        }

        public async void ExecuteUpdateAssets() {
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Updating Assets..."));
            await _shellService.UpdateAssetsAsync().ConfigureAwait(false);
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Assets Updated"));
        }

        public async void ExecuteUpdateTransactions() {
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Updating Transactions..."));
            await _shellService.UpdateTransactionsAsync().ConfigureAwait(false);
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Transactions Updated"));
        }

        public async void ExecuteUpdateJournal() {
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Updating Journal..."));
            await _shellService.UpdateJournalAsync().ConfigureAwait(false);
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Journal Updated"));
        }

        public async void ExecuteUpdateRefTypes() {
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Updating Reference Types..."));
            await _shellService.UpdateRefIdsAsync().ConfigureAwait(false);
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Reference Types Updated"));
        }

        public async void ExecuteRecalculateInventory() {
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Updating inventory..."));
            await _shellService.CalculateInventoryAsync();
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Inventory updated"));
        }
    }
}