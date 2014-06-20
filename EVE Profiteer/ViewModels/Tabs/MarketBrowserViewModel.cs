using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using DevExpress.Xpf.Charts;
using DevExpress.Xpf.Mvvm;
using eZet.EveProfiteer.Events;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;
using eZet.EveProfiteer.Util;

namespace eZet.EveProfiteer.ViewModels.Tabs {
    public class MarketBrowserViewModel : ViewModel, IHandle<ViewMarketDetailsEvent> {
        private readonly MarketBrowserService _marketBrowserService;
        private readonly IEventAggregator _eventAggregator;
        private int _dayLimit = 5;
        private MarketBrowserItem _marketBrowserData;
        private InvType _selectedItem;
        private ICollection<MapRegion> _regions;
        private ICollection<InvType> _invTypes;
        private MapRegion _selectedRegion;
        private BindableCollection<TreeNode> _treeRootNodes;
        private DateTime _viewStart;
        private DateTime _viewEnd;

        public MarketBrowserViewModel(MarketBrowserService marketBrowserService, IEventAggregator eventAggregator) {
            _marketBrowserService = marketBrowserService;
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
            DisplayName = "Market Browser";

            SelectItemCommand = new DelegateCommand<TreeNode>(ExecuteSelectItem);
            AddToOrdersCommand = new DelegateCommand(ExecuteAddToOrders, CanAddToOrders);
            ViewTradeDetailsCommand = new DelegateCommand(ExecuteViewTradeDetails, CanViewTradeDetails);
            ViewEnd = DateTime.UtcNow.Date;
            ViewStart = ViewEnd.AddMonths(-6).Date;
            PropertyChanged += OnPropertyChanged;
            DateTimeMeasurement = DateTimeMeasurementUnit.Day;
        }


        public ICommand AddToOrdersCommand { get; private set; }

        public ICommand ViewTradeDetailsCommand { get; private set; }

        public ICommand SelectItemCommand { get; private set; }

        public DateTime ViewStart {
            get { return _viewStart; }
            set {
                if (value.Equals(_viewStart)) return;
                _viewStart = value;
                NotifyOfPropertyChange(() => ViewStart);
            }
        }

        public DateTime ViewEnd {
            get { return _viewEnd; }
            set {
                if (value.Equals(_viewEnd)) return;
                _viewEnd = value;
                NotifyOfPropertyChange(() => ViewEnd);
            }
        }

        public DateTimeMeasurementUnit DateTimeMeasurement { get; set; }

        public ICollection<InvType> InvTypes {
            get { return _invTypes; }
            private set {
                if (Equals(value, _invTypes)) return;
                _invTypes = value;
                NotifyOfPropertyChange(() => InvTypes);
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

        public ICollection<MapRegion> Regions {
            get { return _regions; }
            private set {
                if (Equals(value, _regions)) return;
                _regions = value;
                NotifyOfPropertyChange(() => Regions);
            }
        }

        public MapRegion SelectedRegion {
            get { return _selectedRegion; }
            private set {
                if (Equals(value, _selectedRegion)) return;
                _selectedRegion = value;
                NotifyOfPropertyChange(() => SelectedRegion);
            }
        }

        public BindableCollection<TreeNode> TreeRootNodes {
            get { return _treeRootNodes; }
            private set {
                if (Equals(value, _treeRootNodes)) return;
                _treeRootNodes = value;
                NotifyOfPropertyChange(() => TreeRootNodes);
            }
        }

        public MarketBrowserItem MarketBrowserData {
            get { return _marketBrowserData; }
            private set {
                if (Equals(value, _marketBrowserData)) return;
                _marketBrowserData = value;
                NotifyOfPropertyChange(() => MarketBrowserData);
            }
        }

        public int DayLimit {
            get { return _dayLimit; }
            set {
                if (_dayLimit == value) return;
                _dayLimit = value;
                NotifyOfPropertyChange(() => DayLimit);
            }
        }

        public void Handle(ViewMarketDetailsEvent message) {
            LoadMarketDetails(message.InvType);
        }

        private bool CanViewTradeDetails() {
            return MarketBrowserData != null && MarketBrowserData.InvType != null;
        }

        private void ExecuteViewTradeDetails() {
            _eventAggregator.PublishOnUIThread(new ViewTransactionDetailsEvent(MarketBrowserData.InvType));
        }

        private void ExecuteAddToOrders() {
            var e = new AddToOrdersEventArgs(MarketBrowserData.InvType);
            _eventAggregator.PublishOnUIThread(e);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs) {
            if (propertyChangedEventArgs.PropertyName == "SelectedItem")
                LoadMarketDetails(SelectedItem);
        }

        private async void LoadMarketDetails(InvType invType) {
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Loading market details..."));
            MarketBrowserData = await GetMarketDetails(SelectedRegion, invType);
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Market details loaded"));
        }

        private void ExecuteSelectItem(TreeNode node) {
            if (node == null || node.InvType == null) return;
            SelectedItem = InvTypes.Single(t => t.TypeId == node.InvType.TypeId);
        }

        public override async Task InitAsync() {
            Regions = await _marketBrowserService.GetRegions();
            SelectedRegion = Regions.Single(region => region.RegionId == ConfigManager.DefaultRegion);
            InvTypes = await _marketBrowserService.GetMarketTypes();
            TreeRootNodes = await _marketBrowserService.GetMarketTree();
        }


        private bool CanAddToOrders() {
            return MarketBrowserData.InvType.Orders.All(
                order => order.ApiKeyEntity_Id != ApplicationHelper.ActiveKeyEntity.Id);
        }

        private async Task<MarketBrowserItem> GetMarketDetails(MapRegion region, InvType invType) {
            return await _marketBrowserService.GetMarketDetails(region, invType);
        }
    }
}