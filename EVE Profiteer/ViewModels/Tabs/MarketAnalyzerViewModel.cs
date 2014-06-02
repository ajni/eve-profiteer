using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Caliburn.Micro;
using DevExpress.Xpf.Mvvm;
using eZet.EveProfiteer.Events;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;
using eZet.EveProfiteer.Util;
using eZet.EveProfiteer.ViewModels.Dialogs;

namespace eZet.EveProfiteer.ViewModels.Tabs {
    public class MarketAnalyzerViewModel : Screen, IHandle<OrdersChangedEventArgs> {
        private readonly EveProfiteerDataService _dataService;
        private readonly EveMarketService _eveMarketService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IWindowManager _windowManager;
        private int _dayLimit = 10;
        private BindableCollection<MarketAnalyzerEntry> _marketAnalyzerResults;
        private ICollection<MapRegion> _regions;
        private BindableCollection<TreeNode> _selectedItems;
        private MapRegion _selectedRegion;
        private StaStation _selectedStation;
        private ICollection<StaStation> _stations;
        private BindableCollection<InvMarketGroup> _treeRootNodes;
        private BindableCollection<TreeNode> _betterTreeRootNodes;

        public MarketAnalyzerViewModel(IWindowManager windowManager, IEventAggregator eventAggregator,
            EveProfiteerDataService dataService,
            EveMarketService eveMarketService) {
            _eveMarketService = eveMarketService;
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _dataService = dataService;
            _eventAggregator.Subscribe(this);
            DisplayName = "Market Analyzer";


            SelectedItems = new BindableCollection<TreeNode>();

            AnalyzeCommand = new DelegateCommand(ExecuteAnalyze, () => SelectedItems.Count != 0);
            AddToOrdersCommand = new DelegateCommand<ICollection<object>>(executeAddToOrders, canAddToOrders);
            LoadOrdersCommand = new DelegateCommand(ExecuteLoadOrders);
            ViewTradeDetailsCommand =
                new DelegateCommand<MarketAnalyzerEntry>(
                    entry => _eventAggregator.PublishOnUIThread(new ViewTradeDetailsEventArgs(entry.InvType)),
                    entry => entry != null);
            ViewMarketDetailsCommand =
                new DelegateCommand<MarketAnalyzerEntry>(
                    entry => _eventAggregator.PublishOnUIThread(new ViewMarketDetailsEventArgs(entry.InvType)),
                    entry => entry != null);
            ViewOrderCommand =
                new DelegateCommand<MarketAnalyzerEntry>(
                    entry => _eventAggregator.PublishOnUIThread(new ViewOrderEventArgs(entry.InvType)), hasValidOrder);

            PropertyChanged += OnPropertyChanged;
        }

        public ICommand ViewMarketDetailsCommand { get; private set; }

        public ICommand ViewOrderCommand { get; private set; }

        public ICommand AddToOrdersCommand { get; private set; }

        public ICommand AnalyzeCommand { get; private set; }

        public ICommand LoadOrdersCommand { get; private set; }

        public ICommand ViewTradeDetailsCommand { get; private set; }

        public BindableCollection<InvMarketGroup> TreeRootNodes {
            get { return _treeRootNodes; }
            private set {
                if (Equals(value, _treeRootNodes)) return;
                _treeRootNodes = value;
                NotifyOfPropertyChange(() => TreeRootNodes);
            }
        }

        public BindableCollection<TreeNode> BetterTreeRootNodes {
            get { return _betterTreeRootNodes; }
            private set {
                if (Equals(value, _betterTreeRootNodes)) return;
                _betterTreeRootNodes = value;
                NotifyOfPropertyChange(() => BetterTreeRootNodes);
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

        public BindableCollection<TreeNode> SelectedItems {
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
            foreach (Order order in ordersChangedEventArgs.Orders) {
                if (lookup.Contains(order.TypeId)) {
                    lookup[order.TypeId].Single().Order = order;
                    MarketAnalyzerResults.NotifyOfPropertyChange(null);
                }
            }
        }

        public bool CanScannerLinkAction() {
            return MarketAnalyzerResults != null;
        }

        public void ScannerLinkAction() {
            //IEnumerable<long> list =
            //    MarketAnalyzerResults.Cast<MarketAnalyzerResult>()
            //        .Where(result => result.IsChecked)
            //        .Select(f => f.TypeId);
            //Uri uri = _eveMarketService.GetScannerLink(list.ToList());
            //var scannerVm = new ScannerLinkViewModel(uri);
            //_windowManager.ShowDialog(scannerVm);
        }

        public async Task InitAsync() {
            //TreeRootNodes = await _dataService.BuildMarketTree(treeViewCheckBox_PropertyChanged);
            BetterTreeRootNodes = await _dataService.BuildBetterMarketTree(treeViewCheckBox_PropertyChanged);
            BetterTreeRootNodes.CollectionChanged += BetterTreeRootNodesOnCollectionChanged;
            Regions = await _dataService.Db.MapRegions.AsNoTracking().OrderBy(region => region.RegionName).ToListAsync();
            SelectedRegion = Regions.Single(f => f.RegionId == ConfigManager.DefaultRegion);
            Stations = SelectedRegion.StaStations.OrderByDescending(f => f.StationName).ToList();
            SelectedStation = Stations.Single(station => station.StationId == ConfigManager.DefaultStation);
        }

        private void BetterTreeRootNodesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs) {

            throw new NotImplementedException();
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
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Analyzing..."));
            var cts = new CancellationTokenSource();
            var progressVm = new AnalyzerProgressViewModel(cts);
            MarketAnalyzer res = await GetMarketAnalyzer(SelectedItems.Select(t => t.InvType).ToList());
            LoadOrderData(res.Result);
            MarketAnalyzerResults = new BindableCollection<MarketAnalyzerEntry>(res.Result);
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Analysis complete"));
        }

        private void LoadOrderData(IEnumerable<MarketAnalyzerEntry> items) {
            items.Apply(
                item =>
                    item.Order =
                        item.InvType.Orders.SingleOrDefault(
                            order => order.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id));
        }

    


        private bool canAddToOrders(ICollection<object> objects) {
            if (objects == null || !objects.Any())
                return false;
            List<MarketAnalyzerEntry> items = objects.Select(item => (MarketAnalyzerEntry) item).ToList();
            return items.All(item => item.Order == null);
        }

        private void executeAddToOrders(ICollection<object> objects) {
            List<InvType> items = objects.Select(item => ((MarketAnalyzerEntry) item).InvType).ToList();
      _eventAggregator.PublishOnUIThread(new AddToOrdersEventArgs(items));
        }

        private async void ExecuteLoadOrders() {
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Analyzing orders..."));
            List<InvType> items =
                _dataService.Db.Orders.Where(order => order.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id)
                    .Select(order => order.InvType).ToList();
            MarketAnalyzer res = await GetMarketAnalyzer(items);
            LoadOrderData(res.Result);
            MarketAnalyzerResults = new BindableCollection<MarketAnalyzerEntry>(res.Result);
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Analysis complete"));
        }

        private async Task<MarketAnalyzer> GetMarketAnalyzer(ICollection<InvType> items) {
            return await
                Task.Run(() => _eveMarketService.AnalyzeMarket(SelectedRegion, SelectedStation, items, DayLimit));
        }

        private void treeViewCheckBox_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            var item = sender as TreeNode;
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