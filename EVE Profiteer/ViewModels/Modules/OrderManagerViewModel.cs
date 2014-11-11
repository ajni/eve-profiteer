﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Caliburn.Micro;
using DevExpress.Mvvm;
using DevExpress.Xpf.Grid;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;
using eZet.EveProfiteer.Ui.Events;
using eZet.EveProfiteer.Util;
using eZet.EveProfiteer.ViewModels.Dialogs;

namespace eZet.EveProfiteer.ViewModels.Modules {
    public sealed class OrderManagerViewModel : ModuleViewModel, IHandle<AddOrdersEvent>,
        IHandle<ViewOrderEvent> {
        private readonly IEventAggregator _eventAggregator;
        private readonly IWindowManager _windowManager;
        private readonly OrderManagerService _orderManagerService;
        private readonly OrderXmlService _orderXmlService;

        private BindableCollection<OrderViewModel> _orders;
        private BindableCollection<OrderViewModel> _selectedOrders;
        private OrderViewModel _selectedOrder;
        private OrderViewModel _focusedOrder;
        private ICollection<InvType> _marketTypes;
        private int dayLimit;
        private MapRegion selectedRegion;
        private StaStation selectedStation;
        private BindableCollection<MapRegion> regions;
        private BindableCollection<StaStation> stations;

        public OrderManagerViewModel(OrderManagerService orderManagerService, IWindowManager windowManager,
            IEventAggregator eventAggregator,
            OrderXmlService orderXmlService) {
            _orderManagerService = orderManagerService;
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _orderXmlService = orderXmlService;
            _eventAggregator.Subscribe(this);
            Orders = new BindableCollection<OrderViewModel>();
            SelectedOrders = new BindableCollection<OrderViewModel>();
            DayLimit = 10;
            ImportXmlCommand = new DelegateCommand(ExecuteImportXml);
            ExportXmlCommand = new DelegateCommand(ExecuteExportXml);
            EditCommand = new DelegateCommand(ExecuteEdit);
            SaveCommand = new DelegateCommand(ExecuteSave);
            UpdateMarketDataCommand = new DelegateCommand(ExecuteUpdateMarketData);
            ViewTradeDetailsCommand =
                new DelegateCommand(
                    () =>
                        _eventAggregator.PublishOnUIThread(new ViewTransactionDetailsEvent(FocusedOrder.Order.InvType)),
                    () => FocusedOrder != null);
            ViewMarketDetailsCommand =
                new DelegateCommand(
                    () => _eventAggregator.PublishOnUIThread(new ViewMarketBrowserEvent(FocusedOrder.Order.InvType)),
                    () => FocusedOrder != null);
            ViewAssetCommand =
                new DelegateCommand(
                    () => _eventAggregator.PublishOnUIThread(new ViewAssetEvent(FocusedOrder.Order.InvType)));
            ViewMarketOrderCommand =
                new DelegateCommand(
                    () => _eventAggregator.PublishOnUIThread(new ViewMarketOrderEvent(FocusedOrder.Order.InvType)));
            DeleteOrdersCommand = new DelegateCommand(ExecuteDeleteOrders);
            ValidateOrderTypeCommand = new DelegateCommand<GridCellValidationEventArgs>(ExecuteValidateOrderType);
        }


        public ICollection<InvType> MarketTypes {
            get { return _marketTypes; }
            private set {
                if (Equals(value, _marketTypes)) return;
                _marketTypes = value;
                NotifyOfPropertyChange(() => MarketTypes);
            }
        }

        public OrderViewModel FocusedOrder {
            get { return _focusedOrder; }
            set {
                if (Equals(value, _focusedOrder)) return;
                _focusedOrder = value;
                NotifyOfPropertyChange(() => FocusedOrder);
            }
        }

        public OrderViewModel SelectedOrder {
            get { return _selectedOrder; }
            set {
                if (Equals(value, _selectedOrder)) return;
                _selectedOrder = value;
                NotifyOfPropertyChange(() => SelectedOrder);
            }
        }

        public ICommand ViewMarketOrderCommand { get; private set; }

        public ICommand ViewAssetCommand { get; private set; }

        public ICommand ViewMarketDetailsCommand { get; private set; }

        public ICommand ValidateOrderTypeCommand { get; private set; }

        public ICommand ViewTradeDetailsCommand { get; private set; }

        public ICommand DeleteOrdersCommand { get; private set; }

        public int DayLimit {
            get { return dayLimit; }
            set {
                if (value == dayLimit) return;
                dayLimit = value;
                NotifyOfPropertyChange();
            }
        }

        public BindableCollection<MapRegion> Regions {
            get { return regions; }
            private set {
                if (Equals(value, regions)) return;
                regions = value;
                NotifyOfPropertyChange();
            }
        }

        public BindableCollection<StaStation> Stations {
            get { return stations; }
            private set {
                if (Equals(value, stations)) return;
                stations = value;
                NotifyOfPropertyChange();
            }
        }

        public MapRegion SelectedRegion {
            get { return selectedRegion; }
            set {
                if (Equals(value, selectedRegion)) return;
                selectedRegion = value;
                Stations = new BindableCollection<StaStation>(selectedRegion.StaStations.OrderBy(f => f.StationName));
                NotifyOfPropertyChange();
            }
        }

        public StaStation SelectedStation {
            get { return selectedStation; }
            set {
                if (Equals(value, selectedStation)) return;
                selectedStation = value;
                NotifyOfPropertyChange();
            }
        }


        public BindableCollection<OrderViewModel> Orders {
            get { return _orders; }
            private set {
                _orders = value;
                NotifyOfPropertyChange(() => Orders);
            }
        }

        public BindableCollection<OrderViewModel> SelectedOrders {
            get { return _selectedOrders; }
            set {
                _selectedOrders = value;
                NotifyOfPropertyChange(() => SelectedOrders);
            }
        }

        public ICommand ImportXmlCommand { get; private set; }

        public ICommand ExportXmlCommand { get; private set; }

        public ICommand SaveCommand { get; private set; }

        public ICommand EditCommand { get; private set; }

        public ICommand UpdateMarketDataCommand { get; private set; }

        public async void Handle(AddOrdersEvent e) {
            await Activate().ConfigureAwait(false);
            var orders = new List<Order>();
            foreach (InvType item in e.Items) {
                var order = new Order();
                order.AutoProcess = true;
                order.TypeId = item.TypeId;
                order.InvType = item;
                order.IsBuyOrder = e.BuyOrder;
                order.IsSellOrder = e.SellOrder;
                item.Orders.Add(order);
                orders.Add(order);
            }
            await _orderManagerService.LoadMarketDataAsync(orders, SelectedRegion.RegionId, SelectedStation.StationId, DayLimit);
            Orders.AddRange(orders.Select(order => new OrderViewModel(order)));
            SelectedOrders.Clear();
            SelectedOrders.AddRange(orders.Select(order => new OrderViewModel(order)));
            SelectedOrder = Orders.Last();
            FocusedOrder = SelectedOrder;
            _eventAggregator.PublishOnUIThread(new OrdersChangedEventArgs { Added = orders });
            string msg = "Order(s) added";
            if (orders.Count == 1)
                msg = "'" + orders.Single().InvType.TypeName + "' added to Orders";
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs(msg));
        }

        public async void Handle(ViewOrderEvent message) {
            await Activate().ConfigureAwait(false);
            OrderViewModel order = Orders.SingleOrDefault(entry => entry.Order.InvType.TypeId == message.InvType.TypeId);
            FocusedOrder = order;
            SelectedOrder = order;
        }

        protected override async Task OnOpen() {
            MarketTypes = await _orderManagerService.GetMarketTypesAsync().ConfigureAwait(false);
            var orders = await _orderManagerService.GetOrdersAsync().ConfigureAwait(false);
            Orders.AddRange(orders);
            SelectedOrder = Orders.FirstOrDefault();
            FocusedOrder = Orders.FirstOrDefault();
            Regions = new BindableCollection<MapRegion>(await _orderManagerService.GetRegions().ConfigureAwait(false));
            SelectedRegion = Regions.Single(f => f.RegionId == Properties.Settings.Default.DefaultRegionId);
            SelectedStation = Stations.Single(f => f.StationId == Properties.Settings.Default.DefaultStationId);
        }

        protected override Task OnDeactivate(bool close) {
            if (close) {
                MarketTypes = null;
                Orders.Clear();
                SelectedOrder = null;
                FocusedOrder = null;
            }
            return Task.FromResult(0);
        }

        private void ExecuteValidateOrderType(GridCellValidationEventArgs eventArgs) {
            if (eventArgs.Value == null) {
                eventArgs.IsValid = false;
                eventArgs.SetError("Invalid item.");
                return;
            }
            string value = eventArgs.Value.ToString();
            InvType item = MarketTypes.SingleOrDefault(f => f.TypeName == value);
            if (item == null) {
                eventArgs.IsValid = false;
                eventArgs.SetError("Invalid item.");
            } else {
                if (Orders.SingleOrDefault(order => order.Order.TypeId == item.TypeId) != null) {
                    eventArgs.IsValid = false;
                    eventArgs.SetError("An order for this item already exists.");
                } else {
                    ((OrderViewModel)eventArgs.Row).Order.TypeId = item.TypeId;
                    ((OrderViewModel)eventArgs.Row).Order.InvType = item;
                }
            }
        }

        private async void ExecuteDeleteOrders() {
            var result = await _orderManagerService.RemoveOrdersAsync(SelectedOrders).ConfigureAwait(false);
            foreach (OrderViewModel order in SelectedOrders.ToList()) {
                Orders.Remove(order);
            }
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Order(s) deleted (" + result + ")"));
        }

        public async void ExecuteSave() {
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Saving orders..."));
            var result = await Task.Run(() => _orderManagerService.SaveOrdersAsync(Orders)).ConfigureAwait(false);
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Order(s) saved (" + result + ")"));
        }

        public void ExecuteImportXml() {
            var dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = false;
            dialog.SelectedPath = ConfigManager.OrderXmlPath;
            if (dialog.ShowDialog() == DialogResult.OK) {
                ICollection<Order> orders = _orderXmlService.ImportOrders(dialog.SelectedPath);
                Orders.Clear();
                Orders.AddRange(orders.Select(order => new OrderViewModel(order)));
                ConfigManager.OrderXmlPath = dialog.SelectedPath;
            }
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Order(s) imported"));
        }

        public void ExecuteExportXml() {
            var dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = false;
            dialog.SelectedPath = Properties.Settings.Default.OrderXmlPath;
            if (dialog.ShowDialog() == DialogResult.OK) {
                _orderXmlService.ExportOrders(dialog.SelectedPath, Orders.Select(entry => entry.Order));
                Properties.Settings.Default.OrderXmlPath = dialog.SelectedPath;
                Properties.Settings.Default.Save();
            }
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Order(s) exported"));
        }

        public async void ExecuteUpdateMarketData() {
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Fetching market data..."));
            await _orderManagerService.LoadMarketDataAsync(Orders.Select(entry => entry.Order), SelectedRegion.RegionId, SelectedStation.StationId, DayLimit);
            Orders.Refresh();
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Market data updated"));
        }


        /// <summary>
        /// Processes and optimizes orders
        /// </summary>
        public void ExecuteEdit() {
            var vm = IoC.Get<OrderOptimizerViewModel>();
            var result = _windowManager.ShowDialog(vm);
            if (result == true) {
                Orders.IsNotifying = false;

                foreach (OrderViewModel order in Orders.Where(o => o.Order.AutoProcess)) {

                    // Price Limits
                    if (vm.EditPriceLimits) {
                        order.MaxBuyPrice = order.AvgPrice * (decimal)(1 + vm.AvgPriceBuyOffset / 100);
                        order.MinSellPrice = order.AvgPrice / (decimal)(1 - vm.AvgPriceSellOffset / 100);
                        if (order.GrossMarginCostAndSell < vm.MinProfitMargin / 100) {
                            if (order.Asset == null || order.TotalQuantity == 0) {
                                order.MinSellPrice = order.MaxBuyPrice / (decimal)(1 - vm.MinProfitMargin / 100);

                            } else {
                                order.MinSellPrice = order.Asset.LatestAverageCost / (decimal)(1 - vm.MinProfitMargin / 100);
                            }
                        }
                    }

                    // Inventory Limit
                    if (vm.EditInventoryLimit) {
                        order.IsBuyOrder = order.InventoryTotalCost < vm.InventoryLimitValue;
                    }

                    // Quantity Limits
                    if (vm.EditOrderQuantities) {
                        if (vm.MaxBuyOrderTotal > 0 && order.MaxBuyPrice > 0) {
                            order.BuyQuantity = (int)(vm.MaxBuyOrderTotal / order.MaxBuyPrice);
                            if (order.MaxBuyPrice > vm.MaxBuyOrderTotal)
                                order.BuyQuantity = 1;

                            // set total as close to target as possible
                            decimal total = order.MaxBuyPrice * order.BuyQuantity;
                            if (vm.MaxBuyOrderTotal - total > total + order.MaxBuyPrice - vm.MaxBuyOrderTotal)
                                order.BuyQuantity += 1;
                        }
                        if (vm.MinSellOrderTotal > 0 && order.MinSellPrice != 0) {
                            order.MinSellQuantity = (int)(vm.MinSellOrderTotal / order.MinSellPrice);
                            if (order.MinSellQuantity == 1) order.MinSellQuantity = 0;
                        }

                        if (vm.MaxSellOrderTotal > 0 && order.MinSellPrice != 0) {
                            order.MaxSellQuantity = (int)(vm.MaxSellOrderTotal / order.MinSellPrice);
                            if (order.MaxSellQuantity == 0)
                                order.MaxSellQuantity = 1;
                        }
                    }
                }
            }
            Orders.IsNotifying = true;
            Orders.Refresh();
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Orders updated"));
        }
    }
}