using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using DevExpress.Mvvm;
using DevExpress.Xpf.Core;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Properties;
using eZet.EveProfiteer.Services;
using eZet.EveProfiteer.Ui.Events;
using eZet.EveProfiteer.ViewModels.Dialogs;

namespace eZet.EveProfiteer.ViewModels.Modules {
    public class AssetManagerViewModel : ModuleViewModel, IHandle<AssetsUpdatedEvent>, IHandle<ViewAssetEvent> {
        private readonly AssetManagerService _assetManagerService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IWindowManager _windowManager;
        private BindableCollection<AssetVm> _assets;
        private AssetVm _focusedRow;
        private ViewAssetEvent _viewAssetEvent;
        private AssetsUpdatedEvent _assetsUpdatedEvent;

        public AssetManagerViewModel(IEventAggregator eventAggregator, IWindowManager windowManager, AssetManagerService assetManagerService) {
            _eventAggregator = eventAggregator;
            _windowManager = windowManager;
            _assetManagerService = assetManagerService;
            MarketHistoryAge = 10;
            Assets = new BindableCollection<AssetVm>();
            _eventAggregator.Subscribe(this);
            ViewMarketDetailsCommand = new DelegateCommand(() => _eventAggregator.PublishOnUIThread(new ViewMarketBrowserEvent(FocusedRow.Asset.invType)));
            ViewTradeDetailsCommand = new DelegateCommand(() => _eventAggregator.PublishOnUIThread(new ViewTransactionDetailsEvent(FocusedRow.Asset.invType)));
            AddQuantityCommand = new DelegateCommand(executeAddQuantity);
            RemoveQuantityCommand = new DelegateCommand(executeRemoveQuantity);
            ResetDataCommand = new DelegateCommand(executeResetData);
            MatchActualQuantityCommand = new DelegateCommand(executeMatchActualQuantity);
            SaveCommand = new DelegateCommand(executeSave);
            RevertCommand = new DelegateCommand(excuteRevert);
            UpdateMarketDataCommand = new DelegateCommand(async () => await UpdateMarketData());
        }



        public ICommand AddQuantityCommand { get; private set; }

        public ICommand UpdateMarketDataCommand { get; private set; }

        private async void excuteRevert() {
            _assetManagerService.Deactivate();
            Assets = new BindableCollection<AssetVm>(await _assetManagerService.GetAssets().ConfigureAwait(false));
            _eventAggregator.PublishOnUIThread(new StatusEvent(this, "Changes Discarded"));
        }

        public ICommand RevertCommand { get; private set; }

        private async void executeSave() {
            await _assetManagerService.Save(Assets).ConfigureAwait(false);
            _eventAggregator.PublishOnUIThread(new StatusEvent(this, "Changed Saved"));
        }

        private void executeMatchActualQuantity() {
            var result = DXMessageBox.Show("Are you sure you want to match the actual quantity for this asset?", "Match Actual Quantity",
    MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes) {
                if (FocusedRow.CalculatedQuantity > 0) {
                    FocusedRow.MaterialCost = FocusedRow.MaterialCost / FocusedRow.CalculatedQuantity *
                                              FocusedRow.ActualQuantity;
                    FocusedRow.BrokerFees = FocusedRow.BrokerFees / FocusedRow.CalculatedQuantity *
                                            FocusedRow.ActualQuantity;
                }
                FocusedRow.CalculatedQuantity = FocusedRow.ActualQuantity;
            }
        }

        private void executeResetData() {
            var result = DXMessageBox.Show("Are you sure you want to reset all data for this asset?", "Reset Asset Data",
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes) {
                FocusedRow.MaterialCost = 0;
                FocusedRow.BrokerFees = 0;
                FocusedRow.CalculatedQuantity = 0;
            }
        }

        private void executeAddQuantity() {
            if (FocusedRow == null) {
                _eventAggregator.PublishOnUIThread(new StatusEvent(this, "No asset selected"));
                return;
            }
            var vm = new AssetAddQuantityDialogViewModel(FocusedRow);
            if (_windowManager.ShowDialog(vm).GetValueOrDefault() && vm.Quantity > 0) {
                FocusedRow.MaterialCost += vm.TransactionCost;
                FocusedRow.CalculatedQuantity += vm.Quantity;
                var mod = new AssetModification();
                mod.AssetId = FocusedRow.Asset.Id;
                mod.Quantity = vm.Quantity;
                mod.Description = vm.Description;
                mod.Date = vm.Date;
                mod.PostModificationQuantity = FocusedRow.CalculatedQuantity;
                mod.TransactionValue = vm.TransactionCost;
                _assetManagerService.AddModification(mod);
                FocusedRow.AssetModifications.Add(mod);
            }
        }

        private void executeRemoveQuantity() {
            if (FocusedRow == null) {
                _eventAggregator.PublishOnUIThread(new StatusEvent(this, "No asset selected"));
                return;
            }
            var vm = new AssetRemoveQuantityDialogViewModel(FocusedRow);
            vm.Quantity = Math.Abs(FocusedRow.CalculatedQuantity);
            if (_windowManager.ShowDialog(vm).GetValueOrDefault() && vm.Quantity > 0) {
                var newQuantity = FocusedRow.CalculatedQuantity - vm.Quantity;
                if (newQuantity < 0) {
                    _eventAggregator.PublishOnUIThread(new StatusEvent(this, "New quantity cannot be less than zero"));
                    return;
                }
                if (newQuantity <= 0) {
                    newQuantity = 0;
                    FocusedRow.BrokerFees = 0;
                    FocusedRow.MaterialCost = 0;
                } else {
                    FocusedRow.BrokerFees = (FocusedRow.BrokerFees / FocusedRow.CalculatedQuantity) * newQuantity;
                    FocusedRow.MaterialCost = (FocusedRow.MaterialCost / FocusedRow.CalculatedQuantity) * newQuantity;
                }
                FocusedRow.CalculatedQuantity = newQuantity;
                var mod = new AssetModification();
                mod.Quantity = 0 - vm.Quantity;
                mod.Description = vm.Description;
                mod.Date = vm.Date;
                mod.AssetId = FocusedRow.Asset.Id;
                mod.PostModificationQuantity = newQuantity;
                _assetManagerService.AddModification(mod);
                FocusedRow.AssetModifications.Add(mod);
            }
        }

        public int MarketHistoryAge { get; set; }

        public BindableCollection<AssetVm> Assets {
            get { return _assets; }
            private set {
                if (Equals(value, _assets)) return;
                _assets = value;
                NotifyOfPropertyChange(() => Assets);
            }
        }


        public AssetVm FocusedRow {
            get { return _focusedRow; }
            set {
                if (Equals(value, _focusedRow)) return;
                _focusedRow = value;
                NotifyOfPropertyChange(() => FocusedRow);
            }
        }


        public ICommand ViewMarketDetailsCommand { get; private set; }

        public ICommand ViewTradeDetailsCommand { get; private set; }

        public ICommand RemoveQuantityCommand { get; private set; }
        public ICommand ResetDataCommand { get; private set; }
        public ICommand MatchActualQuantityCommand { get; private set; }

        public ICommand SaveCommand { get; private set; }


        public async void Handle(AssetsUpdatedEvent message) {
            if (IsActive) {
                await refresh();
            } else {
                _assetsUpdatedEvent = message;
            }
        }

        protected override async Task OnOpen() {
            await LoadAssets().ConfigureAwait(false);
        }

        protected override Task OnDeactivate(bool close) {
            if (close) {
                Assets = null;
                _assetManagerService.Deactivate();
            }
            return base.OnDeactivate(close);
        }

        private async Task refresh() {
            if (_assetsUpdatedEvent != null) {
                await OnOpen();
                _assetsUpdatedEvent = null;
            }
        }


        protected async override Task OnActivate() {
            await refresh();
            if (_viewAssetEvent != null) {
                setFocus(_viewAssetEvent.InvType);
                _viewAssetEvent = null;
            }
        }

        private async Task UpdateMarketData() {
            _eventAggregator.PublishOnUIThread(new StatusEvent(this, "Updating Market Data..."));
            Assets.IsNotifying = false;
            await _assetManagerService.UpdateMarketData(Assets, Settings.Default.DefaultRegionId,
                Settings.Default.DefaultStationId, MarketHistoryAge).ConfigureAwait(false);
            Assets.IsNotifying = true;
            Assets.Refresh();
            _eventAggregator.PublishOnUIThread(new StatusEvent(this, "Market Data Updated"));
        }

        private async Task LoadAssets() {
            _eventAggregator.PublishOnUIThread(new StatusEvent(this, "Loading Assets..."));
            Assets = new BindableCollection<AssetVm>(await _assetManagerService.GetAssets().ConfigureAwait(false));
            _eventAggregator.PublishOnUIThread(new StatusEvent(this, "Assets Loaded"));

        }

        public void Handle(ViewAssetEvent message) {
            if (IsActive)
                setFocus(message.InvType);
            else
                _viewAssetEvent = message;
        }

        private void setFocus(InvType invType) {
            FocusedRow = Assets.SingleOrDefault(f => f.TypeId == invType.TypeId);
        }
    }
}