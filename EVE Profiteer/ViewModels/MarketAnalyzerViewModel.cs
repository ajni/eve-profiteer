using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using DevExpress.Xpf.Mvvm;
using eZet.Eve.EveProfiteer.ViewModels;
using eZet.EveOnlineDbModels;
using eZet.EveProfiteer.Events;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;
using eZet.EveProfiteer.Views;
using Xceed.Wpf.Toolkit;

namespace eZet.EveProfiteer.ViewModels {
    public class MarketAnalyzerViewModel : Screen, IHandle<OrdersAddedEventArgs> {
        private readonly EveMarketService _eveMarketService;
        private readonly EveOnlineStaticDataService _eveOnlineDbService;
        private readonly IEventAggregator _eventAggregator;
        private readonly OrderEditorService _orderEditorService;
        private readonly IWindowManager _windowManager;
        private int _dayLimit = 5;
        private BindableCollection<MarketAnalyzerEntry> _marketAnalyzerResults;
        private BindableCollection<InvType> _selectedItems;
        private Station _selectedStation;

        public MarketAnalyzerViewModel(IWindowManager windowManager, IEventAggregator eventAggregator,
            EveOnlineStaticDataService eveOnlineDbService,
            EveMarketService eveMarketService, OrderEditorService orderEditorService) {
            _eveOnlineDbService = eveOnlineDbService;
            _eveMarketService = eveMarketService;
            _orderEditorService = orderEditorService;
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
            DisplayName = "Market Analyzer";


            SelectedItems = new BindableCollection<InvType>();


            AddToOrdersCommand = new DelegateCommand<ICollection<object>>(AddToOrders, CanAddToOrders);

        }

        protected override void OnInitialize() {
            TreeRootNodes = buildTree();
            Stations = getStations();
            SelectedStation = Stations.Single(f => f.StationId == 60003760);
        }


        public ICommand AddToOrdersCommand { get; set; }


        public ICollection<InvMarketGroup> TreeRootNodes { get; private set; }

        public ICollection<Station> Stations { get; private set; }

        public Station SelectedStation {
            get { return _selectedStation; }
            set {
                if (_selectedStation == value) return;
                _selectedStation = value;
                NotifyOfPropertyChange(() => SelectedStation);
            }
        }

        public BindableCollection<InvType> SelectedItems {
            get { return _selectedItems; }
            private set {
                if (_selectedItems == value) return;
                _selectedItems = value;
                NotifyOfPropertyChange(() => SelectedItems);
            }
        }


        public BindableCollection<MarketAnalyzerEntry> MarketAnalyzerResults {
            get { return _marketAnalyzerResults; }
            private set {
                if (_marketAnalyzerResults == value) return;
                _marketAnalyzerResults = value;
                NotifyOfPropertyChange(() => MarketAnalyzerResults);
            }
        }

        public int DayLimit {
            get { return _dayLimit; }
            private set {
                if (_dayLimit == value) return;
                _dayLimit = value;
                NotifyOfPropertyChange(() => DayLimit);
            }
        }

        public bool CanAnalyzeAction {
            get { return SelectedItems.Count != 0; }
        }

        public async Task AnalyzeAction() {
            var busy = new BusyIndicator { IsBusy = true };
            var cts = new CancellationTokenSource();
            var progressVm = new AnalyzerProgressViewModel(cts);
            MarketAnalyzer res = await GetMarketAnalyzer(SelectedItems);
            LoadOrderData(res.Result);
            MarketAnalyzerResults = new BindableCollection<MarketAnalyzerEntry>(res.Result);
            busy.IsBusy = false;
        }

        private void LoadOrderData(IEnumerable<MarketAnalyzerEntry> items) {
            IQueryable<Order> orders = _orderEditorService.GetOrders();
            ILookup<int, MarketAnalyzerEntry> lookup = items.ToLookup(f => f.InvType.TypeId);
            foreach (Order order in orders) {
                if (lookup.Contains(order.TypeId)) {
                    lookup[order.TypeId].Single().Order = order;
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


        private bool CanAddToOrders(ICollection<object> objects) {
            if (objects == null || !objects.Any())
                return false;
            List<MarketAnalyzerEntry> items = objects.Select(item => (MarketAnalyzerEntry)item).ToList();
            return items.All(item => item.Order == null);
        }

        private void AddToOrders(ICollection<object> objects) {
            if (objects == null || !objects.Any())
                return;
            List<MarketAnalyzerEntry> items = objects.Select(item => (MarketAnalyzerEntry)item).ToList();
            _eventAggregator.Publish(new AddToOrdersEventArgs(items));
        }

        public async Task LoadOrders() {
            var orders = _orderEditorService.GetOrders().Select(item => item.TypeId).ToList();
            List<InvType> items =
                _eveOnlineDbService.GetTypes().Where(item => orders.Contains(item.TypeId)).ToList();
            MarketAnalyzer res = await GetMarketAnalyzer(items);
            LoadOrderData(res.Result);
            MarketAnalyzerResults = new BindableCollection<MarketAnalyzerEntry>(res.Result);
        }
        private async Task<MarketAnalyzer> GetMarketAnalyzer(ICollection<InvType> items) {
            return await
                Task.Run(() => _eveMarketService.GetMarketAnalyzer(SelectedStation, items, DayLimit));
        }

        private ICollection<InvMarketGroup> buildTree() {
            var rootList = new List<InvMarketGroup>();
            _eveOnlineDbService.SetLazyLoad(false);
            List<InvType> items = _eveOnlineDbService.GetTypes().Where(p => p.MarketGroupId.HasValue).ToList();
            List<InvMarketGroup> groupList = _eveOnlineDbService.GetMarketGroups().ToList();
            Dictionary<int, InvMarketGroup> groups = groupList.ToDictionary(t => t.MarketGroupId);

            foreach (InvType item in items) {
                InvMarketGroup group;
                int id = item.MarketGroupId ?? default(int);
                groups.TryGetValue(id, out group);
                group.Children.Add(item);
                item.PropertyChanged += treeViewCheckBox_PropertyChanged;
            }
            foreach (var key in groups) {
                if (key.Value.ParentGroupId.HasValue) {
                    InvMarketGroup group;
                    int id = key.Value.ParentGroupId ?? default(int);
                    groups.TryGetValue(id, out group);
                    group.Children.Add(key.Value);
                } else {
                    rootList.Add(key.Value);
                }
            }
            _eveOnlineDbService.SetLazyLoad(true);
            return rootList;
        }

        private void treeViewCheckBox_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            var item = sender as InvType;
            if (e.PropertyName == "IsChecked") {
                if (item.IsChecked == true) {
                    SelectedItems.Add(item);
                } else if (item.IsChecked == false)
                    SelectedItems.Remove(item);
                else {
                    throw new NotImplementedException();
                }
            }
            NotifyOfPropertyChange(() => CanAnalyzeAction);
        }

        private ICollection<Station> getStations() {
            var list = new List<Station>();
            list.Add(new Station {
                StationName = "Jita IV - Moon 4 - Caldari Navy Assembly Plant",
                StationId = 60003760,
                RegionId = 10000002
            });
            return list;
        }

        public void Handle(OrdersAddedEventArgs ordersAddedEventArgs) {
            ILookup<int, MarketAnalyzerEntry> lookup = MarketAnalyzerResults.ToLookup(f => f.InvType.TypeId);
            foreach (Order order in ordersAddedEventArgs.Orders) {
                if (lookup.Contains(order.TypeId))
                    lookup[order.TypeId].Single().Order = order;
            }
            ((MarketAnalyzerView)GetView()).MarketAnalyzerGridControl.RefreshData();
        }
    }
}