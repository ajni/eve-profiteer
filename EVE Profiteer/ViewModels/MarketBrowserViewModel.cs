using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using DevExpress.Xpf.Mvvm;
using eZet.EveOnlineDbModels;
using eZet.EveProfiteer.Events;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels {
    public class MarketBrowserViewModel : Screen {
        private readonly EveOnlineStaticDataService _eveOnlineDbService;
        private readonly MarketBrowserService _marketBrowserService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IWindowManager _windowManager;
        private int _dayLimit = 5;
        private InvType _selectedItem;
        private MarketBrowserItem _marketBrowserItem;
        private bool _showDonchianChannel;
        private bool _showDonchianCenter;
        private bool _show5DayAverage;
        private bool _show20DayAverage;
        private bool _showAveragePrice;

        public MarketBrowserItem MarketBrowserItem {
            get { return _marketBrowserItem; }
            private set {
                if (Equals(value, _marketBrowserItem)) return;
                _marketBrowserItem = value;
                NotifyOfPropertyChange(() => MarketBrowserItem);
            }
        }

        public InvType SelectedItem {
            get { return _selectedItem; }
            set {
                if (Equals(value, _selectedItem)) return;
                _selectedItem = value;
                NotifyOfPropertyChange(() => SelectedItem);
            }
        }

        public BindableCollection<MarketOrder> CurrentOrders { get; private set; }

        public BindableCollection<MapRegion> Regions { get; private set; }

        public MapRegion SelectedRegion { get; private set; }

        public MarketBrowserViewModel(IWindowManager windowManager, IEventAggregator eventAggregator,
            EveOnlineStaticDataService eveOnlineDbService,
            MarketBrowserService marketBrowserService) {
            _eveOnlineDbService = eveOnlineDbService;
            _marketBrowserService = marketBrowserService;
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
            DisplayName = "Market Browser";

            SelectItemCommand = new DelegateCommand<InvType>(SelectItem);
            AddToOrdersCommand = new DelegateCommand<ICollection<object>>(AddToOrders, CanAddToOrders);
            ViewTradeDetailsCommand =
                new DelegateCommand<MarketAnalyzerEntry>(
                    entry => _eventAggregator.Publish(new ViewTradeDetailsEventArgs(entry.InvType.TypeId)),
                    entry => entry != null && entry.Order != null);
            PropertyChanged += OnPropertyChanged;
        }

    

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs) {
            if (propertyChangedEventArgs.PropertyName == "SelectedItem")
                LoadMarketDetails(SelectedItem);
        }

        private async void LoadMarketDetails(InvType invType) {
            MarketBrowserItem = await GetMarketDetails(SelectedRegion, invType);

        }

        private void SelectItem(InvType invType) {
            if (invType == null) return;
            SelectedItem = invType;
        }

        public bool ShowDonchianChannel {
            get { return _showDonchianChannel; }
            set {
                if (value.Equals(_showDonchianChannel)) return;
                _showDonchianChannel = value;
                NotifyOfPropertyChange(() => ShowDonchianChannel);
            }
        }

        public bool ShowDonchianCenter {
            get { return _showDonchianCenter; }
            set {
                if (value.Equals(_showDonchianCenter)) return;
                _showDonchianCenter = value;
                NotifyOfPropertyChange(() => ShowDonchianCenter);
            }
        }

        public bool Show5DayAverage {
            get { return _show5DayAverage; }
            set {
                if (value.Equals(_show5DayAverage)) return;
                _show5DayAverage = value;
                NotifyOfPropertyChange(() => Show5DayAverage);
                if (value)
                    ShowAveragePrice = true;
            }
        }

        public bool Show20DayAverage {
            get { return _show20DayAverage; }
            set {
                if (value.Equals(_show20DayAverage)) return;
                _show20DayAverage = value;
                NotifyOfPropertyChange(() => Show20DayAverage);
                if (value)
                    ShowAveragePrice = true;
            }
        }

        public bool ShowAveragePrice {
            get { return _showAveragePrice; }
            set {
                if (value.Equals(_showAveragePrice)) return;
                _showAveragePrice = value;
                NotifyOfPropertyChange(() => ShowAveragePrice);
                if (!value) {
                    Show20DayAverage = false;
                    Show5DayAverage = false;
                }
            }
        }


        public ICommand AddToOrdersCommand { get; private set; }

        public ICommand ViewTradeDetailsCommand { get; private set; }

        public ICommand SelectItemCommand { get; private set; }


        public BindableCollection<InvMarketGroup> TreeRootNodes { get; private set; }

        public ICollection<Station> Stations { get; private set; }

        public int DayLimit {
            get { return _dayLimit; }
            set {
                if (_dayLimit == value) return;
                _dayLimit = value;
                NotifyOfPropertyChange(() => DayLimit);
            }
        }

        public void Handle(OrdersAddedEventArgs ordersAddedEventArgs) {
       }

        protected override void OnInitialize() {
            TreeRootNodes = buildTree();
            Regions = new BindableCollection<MapRegion>(_eveOnlineDbService.GetRegions().ToList());
            SelectedRegion = Regions.Single(region => region.RegionId == 10000002);
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

        private async Task<MarketBrowserItem> GetMarketDetails(MapRegion region, InvType invType) {
            return await
                Task.Run(() => _marketBrowserService.GetDetails(region, invType));
        }

        private BindableCollection<InvMarketGroup> buildTree() {
            var rootList = new BindableCollection<InvMarketGroup>();
            _eveOnlineDbService.SetLazyLoad(false);
            List<InvType> items = _eveOnlineDbService.GetTypes().Where(p => p.MarketGroupId.HasValue).ToList();
            List<InvMarketGroup> groupList = _eveOnlineDbService.GetMarketGroups().ToList();
            Dictionary<int, InvMarketGroup> groups = groupList.ToDictionary(t => t.MarketGroupId);

            foreach (InvType item in items) {
                InvMarketGroup group;
                int id = item.MarketGroupId ?? default(int);
                groups.TryGetValue(id, out group);
                group.Children.Add(item);
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


        private ICollection<Station> getStations() {
            var list = new List<Station>();
            list.Add(new Station {
                StationName = "Jita IV - Moon 4 - Caldari Navy Assembly Plant",
                StationId = 60003760,
                RegionId = 10000002
            });
            return list;
        }
    }

}
