using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using eZet.EveProfiteer.Events;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Properties;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels.Tabs {
    public class AssetsViewModel : ModuleViewModel, IHandle<AssetsUpdatedEvent>, IHandle<ViewAssetEvent> {
        private readonly AssetService _assetService;
        private readonly IEventAggregator _eventAggregator;
        private BindableCollection<AssetViewModel> _assets;
        private AssetViewModel _focusedRow;
        private AssetViewModel _selectedRow;
        private BindableCollection<AssetViewModel> _selectedRows;
        private ViewAssetEvent _viewAssetEvent;
        private AssetsUpdatedEvent _assetsUpdatedEvent;

        public AssetsViewModel(IEventAggregator eventAggregator, AssetService assetService) {
            _eventAggregator = eventAggregator;
            _assetService = assetService;
            DisplayName = "Assets";
            AgeSpan = 10;
            Assets = new BindableCollection<AssetViewModel>();
            _eventAggregator.Subscribe(this);
        }

        public IEventAggregator EventAggregator { get; set; }

        public int AgeSpan { get; set; }

        public BindableCollection<AssetViewModel> Assets {
            get { return _assets; }
            private set {
                if (Equals(value, _assets)) return;
                _assets = value;
                NotifyOfPropertyChange(() => Assets);
            }
        }

        public AssetViewModel SelectedRow {
            get { return _selectedRow; }
            set {
                if (Equals(value, _selectedRow)) return;
                _selectedRow = value;
                NotifyOfPropertyChange(() => SelectedRow);
            }
        }

        public AssetViewModel FocusedRow {
            get { return _focusedRow; }
            set {
                if (Equals(value, _focusedRow)) return;
                _focusedRow = value;
                NotifyOfPropertyChange(() => FocusedRow);
            }
        }

        public BindableCollection<AssetViewModel> SelectedRows {
            get { return _selectedRows; }
            set {
                if (Equals(value, _selectedRows)) return;
                _selectedRows = value;
                NotifyOfPropertyChange(() => SelectedRows);
            }
        }

        public ICommand ViewMarketDetailsCommand { get; private set; }

        public ICommand ViewTradeDetailsCommand { get; private set; }

        public async void Handle(AssetsUpdatedEvent message) {
            if (IsActive) {
                await InitAsync();
            } else {
                _assetsUpdatedEvent = message;
            }
        }

        public async Task InitAsync() {
            await LoadAssets().ConfigureAwait(false);
            await UpdateMarketData().ConfigureAwait(false);
        }

        protected override async void OnInitialize() {
            await InitAsync();
        }

        protected override async void OnActivate() {
            if (_assetsUpdatedEvent != null) {
                await InitAsync();
                _assetsUpdatedEvent = null;
            }
            if (_viewAssetEvent != null) {
                setFocus(_viewAssetEvent.InvType);
                _viewAssetEvent = null;
            }
        }

        private async Task UpdateMarketData() {
            Assets.IsNotifying = false;
            await _assetService.UpdateMarketData(Assets, Settings.Default.DefaultRegionId,
                Settings.Default.DefaultStationId, AgeSpan).ConfigureAwait(false);
            Assets.IsNotifying = true;
            Assets.Refresh();
        }

        private async Task LoadAssets() {
            List<Asset> assets = await _assetService.GetAssets().ConfigureAwait(false);
            Assets.AddRange(assets.Select(asset => new AssetViewModel(asset)));
        }

        public void Handle(ViewAssetEvent message) {
            if (IsActive)
                setFocus(message.InvType);
            else
                _viewAssetEvent = message;
        }

        private void setFocus(InvType invType) {
            FocusedRow = Assets.SingleOrDefault(f => f.TypeId == invType.TypeId);
            SelectedRow = _focusedRow;
            SelectedRows.Clear();
            SelectedRows.Add(SelectedRow);
        }
    }
}