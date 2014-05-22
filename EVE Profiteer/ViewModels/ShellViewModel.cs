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
    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive, IShell, IHandle<ViewTradeDetailsEventArgs>, IHandle<ViewMarketDetailsEventArgs> {
        private readonly EveApiService _eveApiService;
        private readonly IEventAggregator _eventAggregator;
        private readonly KeyManagementService _keyManagementService;
        private readonly TransactionService _transactionService;
        private readonly IWindowManager _windowManager;

        public ShellViewModel(IWindowManager windowManager, IEventAggregator eventAggregator,
            KeyManagementService keyManagementService,
            TransactionService transactionService, EveApiService eveApiService) {
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _keyManagementService = keyManagementService;
            _transactionService = transactionService;
            _eveApiService = eveApiService;
            ActiveKey = _keyManagementService.AllApiKeys().FirstOrDefault();
            _eventAggregator.Subscribe(this);
            if (ActiveKey != null)
                ActiveKeyEntity = ActiveKey.ApiKeyEntities.Single(f => f.EntityId == 977615922);

            ManageKeysCommand = new DelegateCommand(ManageKeys);
            UpdateTransactionsCommand = new DelegateCommand(UpdateTransactions);
            DisplayName = "EVE Profiteer";
            SelectKey();

        }

        public ICommand ManageKeysCommand { get; private set; }

        public ICommand UpdateTransactionsCommand { get; private set; }

        public static ApiKey ActiveKey { get; private set; }

        public static ApiKeyEntity ActiveKeyEntity { get; private set; }

        protected virtual IDocumentManagerService DocumentManagerService { get { return null; } }

        Dictionary<string, IDocument> documents = new Dictionary<string, IDocument>();

        public void ShowDocument(string p) {
            string[] parameters = p.Split(';');
            ShowDocumentCore(parameters[0], parameters[1]);
        }
        void ShowDocumentCore(string viewName, string title) {
            IDocument document = null;
            if (!documents.TryGetValue(viewName, out document)) {
                document = DocumentManagerService.CreateDocument(viewName, null, this);
                document.Title = title;
                document.DestroyOnClose = false;
                documents[viewName] = document;
            }
            document.Show();
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

                Items.NotifyOfPropertyChange("Items");
                Items.NotifyOfPropertyChange(null);
            }
        }
        public void ManageKeys() {
            _windowManager.ShowDialog(IoC.Get<ManageKeysViewModel>());
        }

        public async void UpdateTransactions() {
            long latest = 0;
            latest = _transactionService.GetLatestId(ActiveKeyEntity);
            IEnumerable<Transaction> list = _eveApiService.GetNewTransactions(ActiveKey, ActiveKeyEntity, latest);
            IList<Transaction> transactions = list as IList<Transaction> ?? list.ToList();
            await Task.Run(() => _transactionService.BulkInsert(transactions));
        }

        public void Handle(ViewTradeDetailsEventArgs message) {
            ActivateItem(Items.Single(item => item.GetType() == typeof(TradeDetailsViewModel)));
        }

        public void Handle(ViewMarketDetailsEventArgs message) {
            ActivateItem(Items.Single(item => item.GetType() == typeof(MarketBrowserViewModel)));
        }
    }
}