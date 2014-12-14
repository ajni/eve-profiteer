using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using DevExpress.Mvvm;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;
using eZet.EveProfiteer.Ui.Events;
using eZet.EveProfiteer.Util;

namespace eZet.EveProfiteer.ViewModels.Modules {
    public sealed class MarketAnalyzerViewModel : ModuleViewModel, IHandle<OrdersChangedEventArgs> {
        private readonly IEventAggregator _eventAggregator;
        private readonly MarketAnalyzerService _marketAnalyzerService;
        private int _dayLimit = 10;
        private BindableCollection<MarketAnalyzerEntry> _marketAnalyzerResults;
        private BindableCollection<MarketTreeNode> _marketTreeNodes;
        private ICollection<MapRegion> _regions;
        private BindableCollection<MarketTreeNode> _selectedItems;
        private MapRegion _selectedRegion;
        private StaStation _selectedStation;
        private ICollection<StaStation> _stations;

        public MarketAnalyzerViewModel(MarketAnalyzerService marketAnalyzerService, IEventAggregator eventAggregator) {
            _marketAnalyzerService = marketAnalyzerService;
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
            SelectedItems = new BindableCollection<MarketTreeNode>();
            AnalyzeCommand = new DelegateCommand(executeAnalyze, () => SelectedItems.Count != 0);
            AddOrderCommand = new DelegateCommand<ICollection<object>>(executeAddOrder, canAddToOrders);
            AnalyzeOrdersCommand = new DelegateCommand(executeAnalyzeOrders);
            ViewTradeDetailsCommand =
                new DelegateCommand<MarketAnalyzerEntry>(
                    entry => _eventAggregator.PublishOnUIThread(new ViewTransactionDetailsEvent(entry.InvType)),
                    entry => entry != null);
            ViewMarketDetailsCommand =
                new DelegateCommand<MarketAnalyzerEntry>(
                    entry => _eventAggregator.PublishOnUIThread(new ViewMarketBrowserEvent(entry.InvType)),
                    entry => entry != null);
            ViewOrderCommand =
                new DelegateCommand<MarketAnalyzerEntry>(
                    entry => _eventAggregator.PublishOnUIThread(new ViewOrderEvent(entry.InvType)), entry => entry != null && entry.Order != null);
            PropertyChanged += onPropertyChanged;
        }

        public ICommand ViewMarketDetailsCommand { get; private set; }

        public ICommand ViewOrderCommand { get; private set; }

        public ICommand AddOrderCommand { get; private set; }

        public ICommand AnalyzeCommand { get; private set; }

        public ICommand AnalyzeOrdersCommand { get; private set; }

        public ICommand ViewTradeDetailsCommand { get; private set; }

        public BindableCollection<MarketTreeNode> MarketTreeNodes {
            get { return _marketTreeNodes; }
            private set {
                if (Equals(value, _marketTreeNodes)) return;
                _marketTreeNodes = value;
                NotifyOfPropertyChange();
            }
        }

        public ICollection<MapRegion> Regions {
            get { return _regions; }
            private set {
                if (Equals(value, _regions)) return;
                _regions = value;
                NotifyOfPropertyChange();
            }
        }

        public ICollection<StaStation> Stations {
            get { return _stations; }
            private set {
                if (Equals(value, _stations)) return;
                _stations = value;
                NotifyOfPropertyChange();
            }
        }

        public MapRegion SelectedRegion {
            get { return _selectedRegion; }
            set {
                if (Equals(_selectedRegion, value)) return;
                _selectedRegion = value;
                NotifyOfPropertyChange();
            }
        }

        public StaStation SelectedStation {
            get { return _selectedStation; }
            set {
                if (Equals(value, _selectedStation)) return;
                _selectedStation = value;
                NotifyOfPropertyChange();
            }
        }

        public BindableCollection<MarketTreeNode> SelectedItems {
            get { return _selectedItems; }
            private set {
                if (Equals(_selectedItems, value)) return;
                _selectedItems = value;
                NotifyOfPropertyChange();
            }
        }

        public BindableCollection<MarketAnalyzerEntry> MarketAnalyzerResults {
            get { return _marketAnalyzerResults; }
            private set {
                if (Equals(_marketAnalyzerResults, value)) return;
                _marketAnalyzerResults = value;
                NotifyOfPropertyChange();
            }
        }

        public int DayLimit {
            get { return _dayLimit; }
            set {
                if (Equals(_dayLimit, value)) return;
                _dayLimit = value;
                NotifyOfPropertyChange();
            }
        }

        public async void Handle(OrdersChangedEventArgs ordersChangedEventArgs) {
            await Initialized;
            ILookup<int, MarketAnalyzerEntry> lookup = MarketAnalyzerResults.ToLookup(f => f.InvType.TypeId);
            foreach (Order order in ordersChangedEventArgs.Added) {
                if (lookup.Contains(order.TypeId)) {
                    lookup[order.TypeId].Single().Order = order;
                }
            }
            MarketAnalyzerResults.Refresh();
        }

        protected override async Task OnInitialize() {
            MarketTreeNodes =
                await
                    _marketAnalyzerService.GetMarketTreeAsync(treeViewCheckBox_PropertyChanged)
                        .ConfigureAwait(false);
            Regions = await _marketAnalyzerService.GetRegionsAsync().ConfigureAwait(false);
            SelectedRegion = Regions.SingleOrDefault(f => f.RegionId == Properties.Settings.Default.DefaultRegionId);
            if (SelectedRegion != null) {
                Stations = SelectedRegion.StaStations.OrderByDescending(f => f.StationName).ToList();
            }
            SelectedStation = Stations.SingleOrDefault(station => station.StationId == Properties.Settings.Default.DefaultStationId);
        }

        private void onPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs) {
            if (propertyChangedEventArgs.PropertyName == "SelectedRegion") {
                Stations = SelectedRegion.StaStations.OrderBy(f => f.StationName).ToList();
                SelectedStation = Stations.FirstOrDefault();
            }
        }

        private async void executeAnalyze() {
            await analyze(SelectedItems.Select(item => item.InvType));
        }

        private bool canAddToOrders(ICollection<object> objects) {
            if (objects == null || !objects.Any())
                return false;
            List<MarketAnalyzerEntry> items = objects.Select(item => (MarketAnalyzerEntry)item).ToList();
            return items.All(item => item.Order == null);
        }

        private void executeAddOrder(ICollection<object> objects) {
            List<InvType> items = objects.Select(item => ((MarketAnalyzerEntry)item).InvType).ToList();
            _eventAggregator.PublishOnUIThread(new AddOrdersEvent(items));
        }

        private async void executeAnalyzeOrders() {
            List<InvType> items = await _marketAnalyzerService.GetInvTypesForOrdersAsync().ConfigureAwait(false);
            await analyze(items).ConfigureAwait(false);
        }

        private async Task analyze(IEnumerable<InvType> items) {
            _eventAggregator.PublishOnUIThread(new StatusEvent(this, "Fetching market data..."));
            ICollection<MarketAnalyzerEntry> result =
                await
                    _marketAnalyzerService.AnalyzeAsync(SelectedRegion, SelectedStation, items, DayLimit)
                        .ConfigureAwait(false);
            MarketAnalyzerResults = new BindableCollection<MarketAnalyzerEntry>(result);
            _eventAggregator.PublishOnUIThread(new StatusEvent(this, "Market analysis complete"));
        }

        private void treeViewCheckBox_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            var item = sender as MarketTreeNode;
            if (e.PropertyName == "IsChecked") {
                if (item.IsChecked == true) {
                    SelectedItems.Add(item);
                } else if (item.IsChecked == false)
                    SelectedItems.Remove(item);
                else {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}