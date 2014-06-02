using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    public class OrderEditorViewModel : Screen, IHandle<AddToOrdersEventArgs>, IHandle<DeleteOrdersEventArgs>,
        IHandle<ViewOrderEventArgs> {
        private readonly EveProfiteerDataService _dataService;
        private readonly EveMarketService _eveMarketService;
        private readonly IEventAggregator _eventAggregator;
        private readonly OrderXmlService _orderXmlService;
        private readonly IWindowManager _windowManager;
        private Order _focusedOrder;
        private ICollection<InvType> _invTypes;


        private BindableCollection<Order> _orders;
        private Order _selectedOrder;

        private BindableCollection<Order> _selectedOrders;

        public OrderEditorViewModel(IWindowManager windowManager, IEventAggregator eventAggregator,
            OrderXmlService orderXmlService, EveProfiteerDataService dataService, EveMarketService eveMarketService) {
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _orderXmlService = orderXmlService;
            _dataService = dataService;
            _eveMarketService = eveMarketService;
            DisplayName = "Order Editor";
            _eventAggregator.Subscribe(this);
            Orders = new BindableCollection<Order>();
            SelectedOrders = new BindableCollection<Order>();
            DayLimit = 10;
            BuyOrderAvgOffset = 2;
            SellOrderAvgOffset = 2;
            ViewTradeDetailsCommand =
                new DelegateCommand(
                    () => _eventAggregator.PublishOnUIThread(new ViewTradeDetailsEventArgs(FocusedOrder.InvType)),
                    () => FocusedOrder != null);
            ViewMarketDetailsCommand =
                new DelegateCommand(
                    () => _eventAggregator.PublishOnUIThread(new ViewMarketDetailsEventArgs(FocusedOrder.InvType)),
                    () => FocusedOrder != null);
            DeleteOrdersCommand = new DelegateCommand(DeleteOrders);
            SaveOrderCommand = new DelegateCommand<RowEventArgs>(ExecuteSaveOrder);
            ValidateOrderTypeCommand = new DelegateCommand<GridCellValidationEventArgs>(ExecuteValidateOrderType);
            ValidateOrderCommand = new DelegateCommand<GridRowValidationEventArgs>(ExecuteValidateOrder);
        }

        public ICollection<InvType> InvTypes {
            get { return _invTypes; }
            private set {
                if (Equals(value, _invTypes)) return;
                _invTypes = value;
                NotifyOfPropertyChange(() => InvTypes);
            }
        }

        public Order FocusedOrder {
            get { return _focusedOrder; }
            set {
                if (Equals(value, _focusedOrder)) return;
                _focusedOrder = value;
                NotifyOfPropertyChange(() => FocusedOrder);
            }
        }

        public Order SelectedOrder {
            get { return _selectedOrder; }
            set {
                if (Equals(value, _selectedOrder)) return;
                _selectedOrder = value;
                NotifyOfPropertyChange(() => SelectedOrder);
            }
        }

        public ICommand ViewMarketDetailsCommand { get; private set; }

        public ICommand ValidateOrderCommand { get; private set; }

        public ICommand ValidateOrderTypeCommand { get; private set; }

        public ICommand ViewTradeDetailsCommand { get; private set; }

        public ICommand DeleteOrdersCommand { get; private set; }

        public ICommand SaveOrderCommand { get; private set; }

        public int DayLimit { get; set; }

        public int BuyOrderAvgOffset { get; set; }

        public int SellOrderAvgOffset { get; set; }

        public BindableCollection<Order> Orders {
            get { return _orders; }
            private set {
                _orders = value;
                NotifyOfPropertyChange(() => Orders);
            }
        }

        public BindableCollection<Order> SelectedOrders {
            get { return _selectedOrders; }
            set {
                _selectedOrders = value;
                NotifyOfPropertyChange(() => SelectedOrders);
            }
        }

        public async Task InitAsync() {
            InvTypes = await _dataService.GetMarketTypes().AsNoTracking().OrderBy(t => t.TypeName).ToListAsync();
            Orders.AddRange(await _dataService.GetOrders().ToListAsync());
            SelectedOrder = Orders.FirstOrDefault();
            FocusedOrder = Orders.FirstOrDefault();
        }

        public void Handle(AddToOrdersEventArgs e) {
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
            _eveMarketService.LoadMarketData(orders, DayLimit);
            Orders.AddRange(orders);
            SelectedOrders.Clear();
            SelectedOrders.AddRange(orders);
            SelectedOrder = Orders.Last();
            FocusedOrder = SelectedOrder;
            //_eventAggregator.PublishOnUIThread(new OrdersChangedEventArgs(orders));
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Order(s) added"));
        }

        public void Handle(DeleteOrdersEventArgs message) {
            throw new NotImplementedException();
        }

        public void Handle(ViewOrderEventArgs message) {
            Order order = Orders.Single(o => o.InvType.TypeId == message.InvType.TypeId);
            FocusedOrder = order;
            SelectedOrder = order;
        }

        private void ExecuteValidateOrder(GridRowValidationEventArgs eventArgs) {
            var order = (Order) eventArgs.Row;
            object value = eventArgs.Value;
            if (order.TypeId == 0) {
                eventArgs.IsValid = false;
                eventArgs.SetError("A valid type is required.");
            }
        }

        private void ExecuteValidateOrderType(GridCellValidationEventArgs eventArgs) {
            string value = eventArgs.Value.ToString();
            InvType item = _dataService.Db.InvTypes.SingleOrDefault(f => f.TypeName == value);
            if (item == null) {
                eventArgs.IsValid = false;
                eventArgs.SetError("Invalid item.");
            }
            else {
                if (Orders.SingleOrDefault(order => order.TypeId == item.TypeId) != null) {
                    eventArgs.IsValid = false;
                    eventArgs.SetError("An order for this item already exists.");
                }
                else {
                    ((Order) eventArgs.Row).TypeId = item.TypeId;
                    ((Order) eventArgs.Row).InvType = item;
                }
            }
        }

        private void ExecuteSaveOrder(RowEventArgs e) {
            //var order = (Order) e.Row;
            //_dataService.Db.Orders.Add(order);
        }

        private void DeleteOrders() {
            _dataService.Db.Orders.RemoveRange(SelectedOrders);
            foreach (Order order in SelectedOrders.ToList()) {
                Orders.Remove(order);
            }
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Order(s) deleted"));
        }

        private void OrdersOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            //if (e.NewItems != null)
            //    _dataService.Db.Orders.Add(e.NewItems.OfType<Order>().Single());
            //if (e.OldItems != null)
            //    _dataService.Db.Orders.RemoveRange(e.OldItems.OfType<Order>());
        }


        protected override void OnInitialize() {
        }

        public void SaveChanges() {
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Saving orders..."));
            List<Order> newOrders = Orders.Where(order => order.Id == 0).ToList();
            newOrders.Apply(order => order.ApiKeyEntity_Id = ApplicationHelper.ActiveKeyEntity.Id);
            _dataService.Db.Orders.AddRange(newOrders);
            _dataService.Db.SaveChanges();
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Order(s) saved"));
        }


        public void ImportXml() {
            var dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = false;
            dialog.SelectedPath = ConfigManager.OrderXmlPath;
            if (dialog.ShowDialog() == DialogResult.OK) {
                ICollection<Order> orders = _orderXmlService.ImportOrders(dialog.SelectedPath);
                _eveMarketService.LoadMarketData(orders, DayLimit);
                Orders.Clear();
                Orders.AddRange(orders);
                ConfigManager.OrderXmlPath = dialog.SelectedPath;
            }
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Order(s) imported"));
        }

        public void ExportXml() {
            var dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = false;
            dialog.SelectedPath = ConfigManager.OrderXmlPath;
            if (dialog.ShowDialog() == DialogResult.OK) {
                _orderXmlService.ExportOrders(dialog.SelectedPath, Orders);
                ConfigManager.OrderXmlPath = dialog.SelectedPath;
            }
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Order(s) exported"));
        }

        public void UpdateMarketData() {
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Fetching market data..."));
            _eveMarketService.LoadMarketData(Orders, DayLimit);
            Orders.Refresh();
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Market data updated"));
        }

        public void UpdatePriceLimits() {
            foreach (Order order in SelectedOrders) {
                order.MaxBuyPrice = order.AvgPrice + order.AvgPrice*(decimal) (BuyOrderAvgOffset/100.0);
                order.MinSellPrice = order.AvgPrice - order.AvgPrice*(decimal) (SellOrderAvgOffset/100.0);
            }
            Orders.Refresh();
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Price limits updated"));
        }


        public void EditOrdersDialog() {
            var vm = new EditOrderDialogViewModel();
            if (_windowManager.ShowDialog(vm) != true) return;
            foreach (Order order in SelectedOrders) {
                if (vm.SetBuyOrderTotal && order.MaxBuyPrice != 0) {
                    order.BuyQuantity = (int) (vm.BuyOrderTotal/order.MaxBuyPrice);
                    if (order.MaxBuyPrice > vm.BuyOrderTotal)
                        order.BuyQuantity = 1;

                    // set total as close to target as possible
                    decimal total = order.MaxBuyPrice*order.BuyQuantity;
                    if (vm.BuyOrderTotal - total > total + order.MaxBuyPrice - vm.BuyOrderTotal)
                        order.BuyQuantity += 1;
                }
                if (vm.SetMinSellOrderTotal && order.MinSellPrice != 0) {
                    order.MinSellQuantity = (int) (vm.MinSellOrderTotal/order.MinSellPrice);
                    if (order.MinSellQuantity == 1) order.MinSellQuantity = 0;
                }

                if (vm.SetMaxSellOrderTotal && order.MinSellPrice != 0) {
                    order.MaxSellQuantity = (int) (vm.MaxSellOrderTotal/order.MinSellPrice);
                    if (order.MaxSellQuantity == 0)
                        order.MaxSellQuantity = 1;
                }
            }
            Orders.NotifyOfPropertyChange(null);
        }
    }
}