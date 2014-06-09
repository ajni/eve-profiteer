using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using eZet.EveProfiteer.Events;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Properties;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels.Tabs {
    public class AssetsViewModel : Screen, IHandle<AssetsUpdatedEvent> {
        private readonly AssetService _assetService;
        private readonly EveProfiteerDataService _dataService;
        private readonly EveApiService _eveApiService;
        private readonly IEventAggregator _eventAggregator;
        private BindableCollection<AssetEntry> _assets;
        private AssetEntry _focusedRow;
        private AssetEntry _selectedRow;
        private BindableCollection<AssetEntry> _selectedRows;

        public AssetsViewModel(IEventAggregator eventAggregator, EveProfiteerDataService dataService,
            EveApiService eveApiService, AssetService assetService) {
            _eventAggregator = eventAggregator;
            _dataService = dataService;
            _eveApiService = eveApiService;
            _assetService = assetService;
            DisplayName = "Assets";
            AgeSpan = 10;
            Assets = new BindableCollection<AssetEntry>();
            _eventAggregator.Subscribe(this);
        }

        public IEventAggregator EventAggregator { get; set; }

        public int AgeSpan { get; set; }

        public BindableCollection<AssetEntry> Assets {
            get { return _assets; }
            private set {
                if (Equals(value, _assets)) return;
                _assets = value;
                NotifyOfPropertyChange(() => Assets);
            }
        }

        public AssetEntry SelectedRow {
            get { return _selectedRow; }
            set {
                if (Equals(value, _selectedRow)) return;
                _selectedRow = value;
                NotifyOfPropertyChange(() => SelectedRow);
            }
        }

        public AssetEntry FocusedRow {
            get { return _focusedRow; }
            set {
                if (Equals(value, _focusedRow)) return;
                _focusedRow = value;
                NotifyOfPropertyChange(() => FocusedRow);
            }
        }

        public BindableCollection<AssetEntry> SelectedRows {
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
            await LoadOrders().ConfigureAwait(false);
            await UpdateMarketData().ConfigureAwait(false);
        }

        protected override async void OnInitialize() {
            //await UpdateMarketData().ConfigureAwait(false);
        }

        private async Task UpdateMarketData() {
            Assets.IsNotifying = false;
            await _assetService.UpdateAssetData(Assets, Settings.Default.DefaultRegionId,
                Settings.Default.DefaultStationId, AgeSpan).ConfigureAwait(false);
            Assets.IsNotifying = true;
            Assets.Refresh();
        }

        public Task InitAsync() {
            return LoadOrders();
        }

        private async Task LoadOrders() {
            List<Asset> assets = await _assetService.GetAssets().ToListAsync().ConfigureAwait(false);
            Assets.AddRange(assets.Select(asset => new AssetEntry(asset)));
        }
    }
}