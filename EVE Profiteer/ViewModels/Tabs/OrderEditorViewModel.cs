using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Caliburn.Micro;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Mvvm;
using eZet.EveProfiteer.Events;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;
using eZet.EveProfiteer.Util;
using eZet.EveProfiteer.ViewModels.Dialogs;
using Screen = Caliburn.Micro.Screen;

namespace eZet.EveProfiteer.ViewModels.Tabs {
    public class OrderEditorViewModel : ModuleViewModel, IHandle<AddToOrdersEventArgs>, IHandle<DeleteOrdersEventArgs>,
        IHandle<ViewOrderEvent> {
        private readonly EveProfiteerDataService _dataService;
        private readonly EveMarketService _eveMarketService;
        private readonly IEventAggregator _eventAggregator;
        private readonly OrderXmlService _orderXmlService;
        private readonly OrderEditorService _orderEditorService;
        private readonly IWindowManager _windowManager;
        private OrderVm _focusedOrder;
        private ICollection<InvType> _invTypes;


        private BindableCollection<OrderVm> _orders;
        private OrderVm _selectedOrder;

        private BindableCollection<OrderVm> _selectedOrders;

        public OrderEditorViewModel(OrderEditorService orderEditorService, IWindowManager windowManager, IEventAggregator eventAggregator,
            OrderXmlService orderXmlService, EveProfiteerDataService dataService, EveMarketService eveMarketService) {
            _orderEditorService = orderEditorService;
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _orderXmlService = orderXmlService;
            _dataService = dataService;
            _eveMarketService = eveMarketService;
            DisplayName = "Order Editor";
            _eventAggregator.Subscribe(this);
            Orders = new BindableCollection<OrderVm>();
            SelectedOrders = new BindableCollection<OrderVm>();
            DayLimit = 10;
            ViewTradeDetailsCommand =
                new DelegateCommand(
                    () => _eventAggregator.PublishOnBackgroundThread(new ViewTransactionDetailsEvent(FocusedOrder.Order.InvType)),
                    () => FocusedOrder != null);
            ViewMarketDetailsCommand =
                new DelegateCommand(
                    () => _eventAggregator.PublishOnBackgroundThread(new ViewMarketBrowserEvent(FocusedOrder.Order.InvType)),
                    () => FocusedOrder != null);
            DeleteOrdersCommand = new DelegateCommand(DeleteOrders);
            ValidateOrderTypeCommand = new DelegateCommand<GridCellValidationEventArgs>(ExecuteValidateOrderType);
        }

        public ICollection<InvType> InvTypes {
            get { return _invTypes; }
            private set {
                if (Equals(value, _invTypes)) return;
                _invTypes = value;
                NotifyOfPropertyChange(() => InvTypes);
            }
        }

        public OrderVm FocusedOrder {
            get { return _focusedOrder; }
            set {
                if (Equals(value, _focusedOrder)) return;
                _focusedOrder = value;
                NotifyOfPropertyChange(() => FocusedOrder);
            }
        }

        public OrderVm SelectedOrder {
            get { return _selectedOrder; }
            set {
                if (Equals(value, _selectedOrder)) return;
                _selectedOrder = value;
                NotifyOfPropertyChange(() => SelectedOrder);
            }
        }

        public ICommand ViewMarketDetailsCommand { get; private set; }

        public ICommand ValidateOrderTypeCommand { get; private set; }

        public ICommand ViewTradeDetailsCommand { get; private set; }

        public ICommand DeleteOrdersCommand { get; private set; }

        public int DayLimit { get; set; }

        public BindableCollection<OrderVm> Orders {
            get { return _orders; }
            private set {
                _orders = value;
                NotifyOfPropertyChange(() => Orders);
            }
        }

        public BindableCollection<OrderVm> SelectedOrders {
            get { return _selectedOrders; }
            set {
                _selectedOrders = value;
                NotifyOfPropertyChange(() => SelectedOrders);
            }
        }


        public async void Handle(AddToOrdersEventArgs e) {
            var orders = new List<Order>();
            foreach (InvType item in e.Items) {
                var order = new Order();
                order.TypeId = item.TypeId;
                order.InvType = item;
                order.IsBuyOrder = e.BuyOrder;
                order.IsSellOrder = e.SellOrder;
                item.Orders.Add(order);
                orders.Add(order);
            }
            await _orderEditorService.LoadMarketDataAsync(orders, DayLimit);
            Orders.AddRange(orders.Select(order => new OrderVm(order)));
            SelectedOrders.Clear();
            SelectedOrders.AddRange(orders.Select(order => new OrderVm(order)));
            SelectedOrder = Orders.Last();
            FocusedOrder = SelectedOrder;
            _eventAggregator.PublishOnUIThread(new OrdersChangedEventArgs { Added = orders });
            string msg = "Order(s) added";
            if (orders.Count == 1)
                msg = "'" + orders.Single().InvType.TypeName + "' added to Orders";
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs(msg));
        }

        public void Handle(DeleteOrdersEventArgs message) {
            throw new NotImplementedException();
        }

        public void Handle(ViewOrderEvent message) {
            OrderVm order = Orders.Single(entry => entry.Order.InvType.TypeId == message.InvType.TypeId);
            FocusedOrder = order;
            SelectedOrder = order;
        }

        public override async Task InitAsync() {
            InvTypes = await _orderEditorService.GetMarketTypes().ConfigureAwait(false);
            List<Order> orders = await _orderEditorService.GetOrders().ConfigureAwait(false);
            Orders.AddRange(orders.Select(order => new OrderVm(order)));
            SelectedOrder = Orders.FirstOrDefault();
            FocusedOrder = Orders.FirstOrDefault();
        }

        private void ExecuteValidateOrderType(GridCellValidationEventArgs eventArgs) {
            if (eventArgs.Value == null) {
                eventArgs.IsValid = false;
                eventArgs.SetError("Invalid item.");
                return;
            }
            string value = eventArgs.Value.ToString();
            InvType item = InvTypes.SingleOrDefault(f => f.TypeName == value);
            if (item == null) {
                eventArgs.IsValid = false;
                eventArgs.SetError("Invalid item.");
            } else {
                if (Orders.SingleOrDefault(order => order.Order.TypeId == item.TypeId) != null) {
                    eventArgs.IsValid = false;
                    eventArgs.SetError("An order for this item already exists.");
                } else {
                    ((OrderVm)eventArgs.Row).Order.TypeId = item.TypeId;
                    ((OrderVm)eventArgs.Row).Order.InvType = item;
                }
            }
        }

        private void DeleteOrders() {
            _dataService.Db.Orders.RemoveRange(SelectedOrders.Select(entry => entry.Order));
            foreach (OrderVm entry in SelectedOrders.ToList()) {
                Orders.Remove(entry);
            }
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Order(s) deleted"));
        }

        public void SaveChanges() {
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Saving orders..."));
            List<OrderVm> newEntries = Orders.Where(order => order.Order.Id == 0).ToList();
            newEntries.Apply(order => order.Order.ApiKeyEntity_Id = ApplicationHelper.ActiveKeyEntity.Id);
            _dataService.Db.Orders.AddRange(newEntries.Select(entry => entry.Order));
            _dataService.Db.SaveChanges();
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Order(s) saved"));
        }


        public void ImportXml() {
            var dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = false;
            dialog.SelectedPath = ConfigManager.OrderXmlPath;
            if (dialog.ShowDialog() == DialogResult.OK) {
                ICollection<Order> orders = _orderXmlService.ImportOrders(dialog.SelectedPath);
                Orders.Clear();
                Orders.AddRange(orders.Select(order => new OrderVm(order)));
                ConfigManager.OrderXmlPath = dialog.SelectedPath;
            }
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Order(s) imported"));
        }

        public void ExportXml() {
            var dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = false;
            dialog.SelectedPath = ConfigManager.OrderXmlPath;
            if (dialog.ShowDialog() == DialogResult.OK) {
                _orderXmlService.ExportOrders(dialog.SelectedPath, Orders.Select(entry => entry.Order));
                ConfigManager.OrderXmlPath = dialog.SelectedPath;
            }
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Order(s) exported"));
        }

        public async void UpdateMarketData() {
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Fetching market data..."));
            await _orderEditorService.LoadMarketDataAsync(Orders.Select(entry => entry.Order), DayLimit);
            Orders.Refresh();
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Market data updated"));
        }

        public void ExecuteEditAll() {
            var vm = IoC.Get<UpdatePriceLimitsViewModel>();
            var result = _windowManager.ShowDialog(vm);
            if (result == true) {
                Orders.IsNotifying = false;
                if (vm.UpdatePriceLimits) {
                    foreach (OrderVm order in Orders) {
                        order.MaxBuyPrice = order.AvgPrice * (decimal)(1 + vm.AvgPriceBuyOffset / 100);
                        order.MinSellPrice = order.AvgPrice * (decimal)(1 + vm.AvgPriceSellOffset / 100);
                        if (order.GrossMarginForLimitPrice > vm.MaxProfitMargin / 100) {
                            if (order.Asset != null)
                                order.MinSellPrice = order.Asset.LatestAverageCost /
                                                     (decimal)(1 - vm.MaxProfitMargin / 100);
                        }
                        if (order.GrossMarginForLimitPrice < vm.MinProfitMargin / 100) {
                            if (order.Asset != null)
                                order.MinSellPrice = order.Asset.LatestAverageCost /
                                                     (decimal)(1 - vm.MinProfitMargin / 100);
                            else order.MinSellPrice = order.AvgPrice;
                        }
                    }
                }
                if (vm.UpdateQuantities) {
                    foreach (OrderVm order in Orders) {
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