using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using DevExpress.Xpf.Mvvm;
using eZet.EveProfiteer.Events;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;
using eZet.EveProfiteer.Util;

namespace eZet.EveProfiteer.ViewModels.Tabs {
    public class MarketAnalyzerViewModel : ModuleViewModel, IHandle<OrdersChangedEventArgs> {
        private readonly EveProfiteerDataService _dataService;
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

        public MarketAnalyzerViewModel(MarketAnalyzerService marketAnalyzerService, IEventAggregator eventAggregator,
            EveProfiteerDataService dataService) {
            _marketAnalyzerService = marketAnalyzerService;
            _eventAggregator = eventAggregator;
            _dataService = dataService;
            _eventAggregator.Subscribe(this);
            DisplayName = "Market Analyzer";
            Category = ModuleCategory.Trade;


            SelectedItems = new BindableCollection<MarketTreeNode>();

            AnalyzeCommand = new DelegateCommand(ExecuteAnalyze, () => SelectedItems.Count != 0);
            AddToOrdersCommand = new DelegateCommand<ICollection<object>>(executeAddToOrders, canAddToOrders);
            AnalyzeOrdersCommand = new DelegateCommand(ExecuteAnalyzeOrders);
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
                    entry => _eventAggregator.PublishOnUIThread(new ViewOrderEvent(entry.InvType)), hasValidOrder);

            PropertyChanged += OnPropertyChanged;
        }

        public ICommand ViewMarketDetailsCommand { get; private set; }

        public ICommand ViewOrderCommand { get; private set; }

        public ICommand AddToOrdersCommand { get; private set; }

        public ICommand AnalyzeCommand { get; private set; }

        public ICommand AnalyzeOrdersCommand { get; private set; }

        public ICommand ViewTradeDetailsCommand { get; private set; }

        public BindableCollection<MarketTreeNode> MarketTreeNodes {
            get { return _marketTreeNodes; }
            private set {
                if (Equals(value, _marketTreeNodes)) return;
                _marketTreeNodes = value;
                NotifyOfPropertyChange(() => MarketTreeNodes);
            }
        }

        public ICollection<MapRegion> Regions {
            get { return _regions; }
            private set {
                if (Equals(value, _regions)) return;
                _regions = value;
                NotifyOfPropertyChange(() => Regions);
            }
        }

        public ICollection<StaStation> Stations {
            get { return _stations; }
            private set {
                if (Equals(value, _stations)) return;
                _stations = value;
                NotifyOfPropertyChange(() => Stations);
            }
        }

        public MapRegion SelectedRegion {
            get { return _selectedRegion; }
            set {
                if (Equals(_selectedRegion, value)) return;
                _selectedRegion = value;
                NotifyOfPropertyChange(() => SelectedRegion);
            }
        }

        public StaStation SelectedStation {
            get { return _selectedStation; }
            set {
                if (Equals(value, _selectedStation)) return;
                _selectedStation = value;
                NotifyOfPropertyChange(() => SelectedStation);
            }
        }

        public BindableCollection<MarketTreeNode> SelectedItems {
            get { return _selectedItems; }
            private set {
                if (Equals(_selectedItems, value)) return;
                _selectedItems = value;
                NotifyOfPropertyChange(() => SelectedItems);
            }
        }


        public BindableCollection<MarketAnalyzerEntry> MarketAnalyzerResults {
            get { return _marketAnalyzerResults; }
            private set {
                if (Equals(_marketAnalyzerResults, value)) return;
                _marketAnalyzerResults = value;
                NotifyOfPropertyChange(() => MarketAnalyzerResults);
            }
        }

        public int DayLimit {
            get { return _dayLimit; }
            set {
                if (Equals(_dayLimit, value)) return;
                _dayLimit = value;
                NotifyOfPropertyChange(() => DayLimit);
            }
        }

        public void Handle(OrdersChangedEventArgs ordersChangedEventArgs) {
            ILookup<int, MarketAnalyzerEntry> lookup = MarketAnalyzerResults.ToLookup(f => f.InvType.TypeId);
            foreach (Order order in ordersChangedEventArgs.Added) {
                if (lookup.Contains(order.TypeId)) {
                    lookup[order.TypeId].Single().Order = order;
                }
            }
            MarketAnalyzerResults.Refresh();
        }

        public override async Task InitAsync() {
            MarketTreeNodes =
                await _marketAnalyzerService.GetMarketTreeAsync(treeViewCheckBox_PropertyChanged).ConfigureAwait(false);
            Regions = await _marketAnalyzerService.GetRegionsAsync().ConfigureAwait(false);
            SelectedRegion = Regions.SingleOrDefault(f => f.RegionId == ConfigManager.DefaultRegion);
            if (SelectedRegion != null) {
                Stations = SelectedRegion.StaStations.OrderByDescending(f => f.StationName).ToList();
            }
            SelectedStation = Stations.SingleOrDefault(station => station.StationId == ConfigManager.DefaultStation);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs) {
            if (propertyChangedEventArgs.PropertyName == "SelectedRegion") {
                Stations = SelectedRegion.StaStations.OrderByDescending(f => f.StationName).ToList();
                SelectedStation = null;
            }
        }

        private bool hasValidOrder(MarketAnalyzerEntry entry) {
            return entry != null &&
                   entry.InvType.Orders.Any(order => order.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id);
        }


        private async void ExecuteAnalyze() {
            await analyze(SelectedItems.Select(item => item.InvType));
        }

        private async Task LoadOrderData(IEnumerable<MarketAnalyzerEntry> items) {
            List<Order> orders = await _dataService.GetOrders().ToListAsync();
            ILookup<int, Order> lookup = orders.ToLookup(order => order.TypeId);
            items.Apply(item => item.Order = lookup[item.InvType.TypeId].SingleOrDefault());
        }


        private bool canAddToOrders(ICollection<object> objects) {
            if (objects == null || !objects.Any())
                return false;
            List<MarketAnalyzerEntry> items = objects.Select(item => (MarketAnalyzerEntry)item).ToList();
            return items.All(item => item.Order == null);
        }

        private void executeAddToOrders(ICollection<object> objects) {
            List<InvType> items = objects.Select(item => ((MarketAnalyzerEntry)item).InvType).ToList();
            _eventAggregator.PublishOnUIThread(new AddToOrdersEventArgs(items));
        }

        private async void ExecuteAnalyzeOrders() {
            List<InvType> items = await _marketAnalyzerService.GetInvTypesForOrdersAsync().ConfigureAwait(false);
            await analyze(items).ConfigureAwait(false);
        }

        private async Task analyze(IEnumerable<InvType> items) {
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Fetching market data..."));

            ICollection<MarketAnalyzerEntry> result =
                await
                    _marketAnalyzerService.AnalyzeAsync(SelectedRegion, SelectedStation, items, DayLimit)
                        .ConfigureAwait(false);
            MarketAnalyzerResults = new BindableCollection<MarketAnalyzerEntry>(result);
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Analyzing market data..."));
            await LoadOrderData(result).ConfigureAwait(false);
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Market analysis complete"));
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