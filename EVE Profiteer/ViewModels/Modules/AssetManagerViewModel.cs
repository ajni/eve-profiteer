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
        private readonly AssetService _assetService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IWindowManager _windowManager;
        private BindableCollection<AssetVm> _assets;
        private AssetVm _focusedRow;
        private AssetVm _selectedRow;
        private BindableCollection<AssetVm> _selectedRows;
        private ViewAssetEvent _viewAssetEvent;
        private AssetsUpdatedEvent _assetsUpdatedEvent;

        public AssetManagerViewModel(IEventAggregator eventAggregator, IWindowManager windowManager, AssetService assetService) {
            _eventAggregator = eventAggregator;
            _windowManager = windowManager;
            _assetService = assetService;
            AgeSpan = 10;
            Assets = new BindableCollection<AssetVm>();
            _eventAggregator.Subscribe(this);
            ViewMarketDetailsCommand = new DelegateCommand(() => _eventAggregator.PublishOnUIThread(new ViewMarketBrowserEvent(FocusedRow.Asset.invType)));
            ViewTradeDetailsCommand = new DelegateCommand(() => _eventAggregator.PublishOnUIThread(new ViewTransactionDetailsEvent(FocusedRow.Asset.invType)));
            UsedForOtherCommand = new DelegateCommand(executeUsedForOther);
            ResetDataCommand = new DelegateCommand(executeResetData);
            MatchActualQuantityCommand = new DelegateCommand(executeMatchActualQuantity);
            SaveCommand = new DelegateCommand(executeSave);
            RevertCommand = new DelegateCommand(excuteRevert);
            UpdateMarketDataCommand = new DelegateCommand(async () => await UpdateMarketData());
        }

        public ICommand UpdateMarketDataCommand { get; private set; }

        private async void excuteRevert() {
            _assetService.Deactivate();
            Assets = new BindableCollection<AssetVm>(await _assetService.GetAssets().ConfigureAwait(false));
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs(this , "Changes Discarded"));
        }

        public ICommand RevertCommand { get; private set; }

        private async void executeSave() {
            await _assetService.Save(Assets).ConfigureAwait(false);
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs(this, "Changed Saved"));
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

        private void executeUsedForOther() {
            var vm = IoC.Get<AssetReductionDialogViewModel>();
            vm.MaxQuantity = Math.Abs(FocusedRow.CalculatedQuantity);
            vm.Quantity = vm.MaxQuantity;
            if (_windowManager.ShowDialog(vm).GetValueOrDefault()) {
                var newQuantity = FocusedRow.CalculatedQuantity - vm.Quantity;
                if (newQuantity >= 0) {
                    newQuantity = 0;
                    FocusedRow.BrokerFees = 0;
                    FocusedRow.MaterialCost = 0;
                } else {
                    FocusedRow.BrokerFees = (FocusedRow.BrokerFees / FocusedRow.CalculatedQuantity) * newQuantity;
                    FocusedRow.MaterialCost = (FocusedRow.MaterialCost / FocusedRow.CalculatedQuantity) * newQuantity;
                }
                FocusedRow.CalculatedQuantity = newQuantity;
                var reduction = new AssetReduction();
                reduction.Quantity = vm.Quantity;
                reduction.Description = vm.Description;
                reduction.AssetId = FocusedRow.Asset.Id;
                reduction.PostReductionQuantity = newQuantity;
                _assetService.AddReduction(reduction);
            }
        }

        public int AgeSpan { get; set; }

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

        public ICommand UsedForOtherCommand { get; private set; }
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
                _assetService.Deactivate();
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
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs(this, "Updating Market Data..."));
            Assets.IsNotifying = false;
            await _assetService.UpdateMarketData(Assets, Settings.Default.DefaultRegionId,
                Settings.Default.DefaultStationId, AgeSpan).ConfigureAwait(false);
            Assets.IsNotifying = true;
            Assets.Refresh();
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs(this, "Market Data Updated"));
        }

        private async Task LoadAssets() {
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs(this, "Loading Assets..."));
            Assets = new BindableCollection<AssetVm>(await _assetService.GetAssets().ConfigureAwait(false));
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs(this, "Assets Loaded"));

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