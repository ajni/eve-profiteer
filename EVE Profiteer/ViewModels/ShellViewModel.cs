using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Mvvm;
using eZet.EveProfiteer.Events;
using eZet.EveProfiteer.Framework;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;
using eZet.EveProfiteer.Util;
using eZet.EveProfiteer.ViewModels.Dialogs;
using eZet.EveProfiteer.ViewModels.Tabs;
using eZet.EveProfiteer.Views;

namespace eZet.EveProfiteer.ViewModels {
    public class ShellViewModel : Conductor<IScreen>.Collection.AllActive, IShell, IHandle<StatusChangedEventArgs>,
        IHandle<IActivateTabEvent> {
        private readonly IEventAggregator _eventAggregator;
        private readonly ShellService _shellService;
        private readonly ModuleService _moduleService;
        private readonly TraceSource _trace = new TraceSource("EveProfiteer", SourceLevels.All);
        private readonly IWindowManager _windowManager;
        private string _statusMessage;
        private int _selectedTabIndex;

        public ShellViewModel(IWindowManager windowManager, IEventAggregator eventAggregator, ShellService shellService, ModuleService moduleService) {
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _shellService = shellService;
            _moduleService = moduleService;
            DisplayName = "EVE Profiteer";
            AllowStatusChange = true;
            _eventAggregator.Subscribe(this);
            StatusMessage = "Initializing...";

            ActiveKey = _shellService.GetApiKeys().Result.First();

            if (ActiveKey != null)
                ActiveKeyEntity = ActiveKey.ApiKeyEntities.Single(f => f.Id == 977615922);
            ApplicationHelper.ActiveKeyEntity = ActiveKeyEntity;
            ApplicationHelper.ActiveKey = ActiveKey;

            ManageKeysCommand = new DelegateCommand(ManageKeys);
            UpdateTransactionsCommand = new DelegateCommand(ExecuteUpdateTransactions);
            ProcessAllTransactionsCommand = new DelegateCommand(ExecuteProcessAllTransactions);
            UpdateAssetsCommand = new DelegateCommand(ExecuteUpdateAssets);
            UpdateJournalCommand = new DelegateCommand(ExecuteUpdateJournal);
            UpdateApiCommand = new DelegateCommand(ExecuteUpdateApi);
            UpdateRefTypesCommand = new DelegateCommand(ExecuteUpdateRefTypes);
            ProcessUnaccountedTransactionsCommand = new DelegateCommand(ExecuteProcessUnaccountedTransactions);
            UpdateIndustryJobsCommand = new DelegateCommand(ExecuteUpdateIndystryJobs);
            UpdateMarketOrdersCommand = new DelegateCommand(ExecuteUpdateMarketOrders);
            ActivateTabCommand = new DelegateCommand<Type>(ExecuteViewTab);
            OpenSettingsCommand = new DelegateCommand(ExecuteOpenSettings);
        }

        private void ExecuteOpenSettings() {
            var result = _windowManager.ShowDialog(IoC.Get<SettingsViewModel>());
            if (result.GetValueOrDefault()) {
                Properties.Settings.Default.Save();
                _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Settings Saved"));
            } else {
                Properties.Settings.Default.Reload();
                _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Settings Discarded"));
            }
        }

        public ICommand OpenSettingsCommand { get; set; }

        private void ExecuteViewTab(Type type) {
            var vm = _moduleService.GetModule(type);
            activateTab(vm);
        }

        private void activateTab(ModuleViewModel viewModel) {
            ActivateItem(viewModel);
            var view = (ShellView)GetView();
            var tab = view.ModuleHost.VisiblePages.SingleOrDefault(page => page.DataContext == viewModel);
            if (tab == null) {
                tab = view.DockLayoutManager.ClosedPanels.Single(page => page.DataContext == viewModel);
                tab.Closed = false;
            }
            view.DockLayoutManager.DockController.Activate(tab);
        }

        private void initDefaultModules() {
            activateTab(_moduleService.GetModule<TradeSummaryViewModel>());
        }

        protected override void OnViewLoaded(object view) {
            var shellView = (ShellView)view;
            initDefaultModules();
            initializeRibbon(shellView);
            base.OnViewLoaded(view);
        }

        private void initializeRibbon(ShellView view) {
            foreach (var vm in _moduleService.Modules) {
                var button = new BarButtonItem();
                button.Command = ActivateTabCommand;
                button.Content = vm.DisplayName;
                button.CommandParameter = vm.GetType();
                button.LargeGlyph = GetImageStream(vm.LargeGlyph);
                button.Glyph = GetImageStream(vm.Glyph);
                switch (vm.Category) {
                    case ModuleViewModel.ModuleCategory.Trade:
                        view.TradeModules.ItemLinks.Add(button);
                        break;
                    case ModuleViewModel.ModuleCategory.Industry:
                        view.IndustryModules.ItemLinks.Add(button);
                        break;
                    case ModuleViewModel.ModuleCategory.Basic:
                        view.MiscModules.ItemLinks.Add(button);
                        break;
                }
            }
        }

        public static BitmapSource GetImageStream(Image myImage) {
            var bitmap = new Bitmap(myImage);
            IntPtr bmpPt = bitmap.GetHbitmap();
            BitmapSource bitmapSource =
             System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                   bmpPt,
                   IntPtr.Zero,
                   Int32Rect.Empty,
                   BitmapSizeOptions.FromEmptyOptions());

            //freeze bitmapSource and clear memory to avoid memory leaks
            bitmapSource.Freeze();
            DeleteObject(bmpPt);

            return bitmapSource;
        }

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr value);

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

        #region COMMANDS

        public ICommand ActivateTabCommand { get; private set; }

        public ICommand UpdateMarketOrdersCommand { get; private set; }

        public ICommand UpdateIndustryJobsCommand { get; private set; }

        public ICommand ProcessUnaccountedTransactionsCommand { get; private set; }

        public ICommand ProcessAllTransactionsCommand { get; private set; }

        public ICommand UpdateAssetsCommand { get; private set; }

        public ICommand ManageKeysCommand { get; private set; }

        public ICommand UpdateTransactionsCommand { get; private set; }

        public ICommand UpdateJournalCommand { get; private set; }

        public ICommand UpdateApiCommand { get; private set; }

        public ICommand UpdateRefTypesCommand { get; private set; }

        #endregion

        public static ApiKey ActiveKey { get; private set; }

        public static ApiKeyEntity ActiveKeyEntity { get; private set; }

        public int SelectedTabIndex {
            get { return _selectedTabIndex; }
            set {
                if (value == _selectedTabIndex) return;
                _selectedTabIndex = value;
                NotifyOfPropertyChange();
            }
        }

        public void Handle(StatusChangedEventArgs message) {
            _trace.TraceEvent(TraceEventType.Information, 0, "StatusChangedEventArgs: {0}", message.Status);
            if (AllowStatusChange) {
                StatusMessage = message.Status;
            }
        }

        public async Task InitAsync() {
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Initializing..."));
            AllowStatusChange = false;
            IList<Task> tasks = new List<Task>();
            Items.Apply(f => tasks.Add(((ModuleViewModel)f).InitAsync()));
            await Task.WhenAll(tasks);
            AllowStatusChange = true;
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Ready"));
        }

        public void ManageKeys() {
            _windowManager.ShowDialog(IoC.Get<ManageKeysViewModel>());
        }

        public async void ExecuteUpdateApi() {
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Updating API..."));
            var assets = Task.Run(() => updateAssets());
            var transactions = Task.Run(() => updateTransactions());
            var journal = Task.Run(() => updateJournal());
            var marketOrders = Task.Run(() => updateMarketOrders());
            await Task.WhenAll(assets, transactions, journal, marketOrders).ConfigureAwait(false);
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("API Update Complete"));
        }

        public async void ExecuteUpdateAssets() {
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Updating Assets..."));
            await updateAssets();
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Assets Updated"));
        }


        public async void ExecuteUpdateTransactions() {
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Updating Transactions..."));
            await updateTransactions();
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Transactions Updated"));
        }


        public async void ExecuteUpdateJournal() {
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Updating Journal..."));
            await updateJournal();
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Journal Updated"));
        }

        public async void ExecuteUpdateMarketOrders() {
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Updating Market Orders..."));
            var result = await updateMarketOrders();
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Market Orders Updated ({0})", result));
        }

        public async void ExecuteUpdateRefTypes() {
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Updating Reference Types..."));
            await _shellService.UpdateRefIdsAsync().ConfigureAwait(false);
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Reference Types Updated"));
        }

        public async void ExecuteProcessAllTransactions() {
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Processing All Transactions..."));
            await _shellService.ProcessAllTransactionsAsync();
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("All Transactions Processed"));
        }


        private async void ExecuteUpdateIndystryJobs() {
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Updating Industry Jobs..."));
            await _shellService.UpdateIndustryJobs();
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Industry Jobs Updated"));
            _eventAggregator.PublishOnBackgroundThread(new IndustryJobsUpdatedEvent());
        }

        private async void ExecuteProcessUnaccountedTransactions() {
            await _shellService.ProcessUnaccountedTransactionsAsync().ConfigureAwait(false);
        }

        private async Task updateAssets() {
            var result = await _shellService.UpdateAssetsAsync().ConfigureAwait(false);
            if (result > 0) {
                _eventAggregator.PublishOnBackgroundThread(new AssetsUpdatedEvent());
            }
        }

        private async Task updateTransactions() {
            var result = await _shellService.UpdateTransactionsAsync().ConfigureAwait(false);
            if (result > 0) {
                _eventAggregator.PublishOnBackgroundThread(new TransactionsUpdatedEvent());
            }
        }

        private async Task updateJournal() {
            var result = await _shellService.UpdateJournalAsync().ConfigureAwait(false);
            if (result > 0) {
                _eventAggregator.PublishOnBackgroundThread(new JournalUpdatedEvent());
            }
        }

        private async Task<int> updateMarketOrders() {
            var result = await _shellService.UpdateMarketOrdersAsync().ConfigureAwait(false);
            if (result > 0) {
                _eventAggregator.PublishOnBackgroundThread(new MarketOrdersUpdatedEvent());
            }
            return result;
        }

        public void Handle(IActivateTabEvent message) {
            ExecuteViewTab(message.GetTabType());
        }
    }
}