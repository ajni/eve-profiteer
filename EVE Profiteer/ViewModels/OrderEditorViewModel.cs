using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using Caliburn.Micro;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Mvvm;
using eZet.EveProfiteer.Events;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;
using eZet.EveProfiteer.Util;
using eZet.EveProfiteer.Views;
using Screen = Caliburn.Micro.Screen;

namespace eZet.EveProfiteer.ViewModels {
    public class OrderEditorViewModel : Screen, IHandle<AddToOrdersEventArgs>, IHandle<GridCellValidationEventArgs>, IHandle<DeleteOrdersEventArgs>, IHandle<ViewOrderEventArgs> {
        private readonly EveProfiteerDataService _dataService;
        private readonly EveMarketService _eveMarketService;
        private readonly IEventAggregator _eventAggregator;
        private readonly OrderXmlService _orderXmlService;
        private readonly IWindowManager _windowManager;


        private BindableCollection<Order> _orders;

        private BindableCollection<Order> _selectedOrders;
        private string _selectedPath = @"C:\Users\Lars Kristian\AppData\Local\MacroLab\Eve Pilot\Client_1\EVETrader";
        private Order _focusedOrder;

        public OrderEditorViewModel(IWindowManager windowManager, IEventAggregator eventAggregator,
            OrderXmlService orderXmlService, EveProfiteerDataService dataService, EveMarketService eveMarketService) {
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _orderXmlService = orderXmlService;
            _dataService = dataService;
            _eveMarketService = eveMarketService;
            DisplayName = "Order Editor";
            _eventAggregator.Subscribe(this);

            SelectedOrders = new BindableCollection<Order>();
            Orders = new BindableCollection<Order>();
            DayLimit = 10;
            BuyOrderAvgOffset = 2;
            SellOrderAvgOffset = 2;
            ViewTradeDetailsCommand = new DelegateCommand<Order>(order => _eventAggregator.Publish(new ViewTradeDetailsEventArgs(order.InvType)));
            DeleteOrdersCommand = new DelegateCommand<ICollection<Order>>(DeleteOrders);
            SaveOrderCommand = new DelegateCommand<RowEventArgs>(ExecuteSaveOrder);

            Orders.AddRange(_dataService.Db.Orders.Where(order => order.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id).ToList());
            Orders.CollectionChanged += OrdersOnCollectionChanged;
            InvTypes = _dataService.Db.InvTypes.Where(type => type.MarketGroupId != null).ToList();
        }

        private void ExecuteSaveOrder(RowEventArgs e) {
            //var order = (Order) e.Row;
            //_dataService.Db.Orders.Add(order);
        }

        private void DeleteOrders(ICollection<Order> orderCollection) {
            var orders = orderCollection.ToList();
            foreach (var order in orders) {
                Orders.Remove(order);
            }
            _dataService.Db.Orders.RemoveRange(orders);
        }

        public ICollection<InvType> InvTypes { get; private set; }

        public Order FocusedOrder {
            get { return _focusedOrder; }
            private set {
                if (Equals(value, _focusedOrder)) return;
                _focusedOrder = value;
                NotifyOfPropertyChange(() => FocusedOrder);
            }
        }

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

        private void OrdersOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            //if (e.NewItems != null)
            //    _dataService.Db.Orders.Add(e.NewItems.OfType<Order>().Single());
            //if (e.OldItems != null)
            //    _dataService.Db.Orders.RemoveRange(e.OldItems.OfType<Order>());
        }


        protected override void OnInitialize() {

        }

        public void SaveChanges() {
            _dataService.Db.Orders.RemoveRange(_dataService.Db.Orders);
            _dataService.Db.SaveChanges();
            Orders.Apply(order => order.ApiKeyEntity_Id = ApplicationHelper.ActiveKeyEntity.Id);
            _dataService.Db.Orders.AddRange(Orders);
            _dataService.Db.SaveChanges();
        }



        public void ImportXml() {
            var dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = false;
            dialog.SelectedPath = _selectedPath;
            if (dialog.ShowDialog() == DialogResult.OK) {
                ICollection<Order> orders = _orderXmlService.ImportOrders(dialog.SelectedPath);
                _eveMarketService.LoadMarketData(orders, DayLimit);
                Orders.Clear();
                Orders.AddRange(orders);
                _selectedPath = dialog.SelectedPath;
            }
        }

        public void ExportXml() {
            var dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = false;
            dialog.SelectedPath = _selectedPath;
            if (dialog.ShowDialog() == DialogResult.OK) {
                _orderXmlService.ExportOrders(dialog.SelectedPath, Orders);
                _selectedPath = dialog.SelectedPath;
            }
        }

        public void UpdateMarketData() {
            _eveMarketService.LoadMarketData(Orders, DayLimit);
            Orders.Refresh();

        }

        public void UpdatePriceLimits() {
            foreach (Order order in SelectedOrders) {
                order.MaxBuyPrice = order.AvgPrice + order.AvgPrice * (decimal)(BuyOrderAvgOffset / 100.0);
                order.MinSellPrice = order.AvgPrice - order.AvgPrice * (decimal)(SellOrderAvgOffset / 100.0);
            }
            Orders.Refresh();
        }


        public void EditOrdersDialog() {
            var vm = new EditOrderDialogViewModel();
            if (_windowManager.ShowDialog(vm) != true) return;
            foreach (Order order in SelectedOrders) {
                if (vm.SetBuyOrderTotal && order.MaxBuyPrice != 0) {
                    order.BuyQuantity = (int)(vm.BuyOrderTotal / order.MaxBuyPrice);
                    if (order.MaxBuyPrice > vm.BuyOrderTotal)
                        order.BuyQuantity = 1;

                    // set total as close to target as possible
                    var total = order.MaxBuyPrice * order.BuyQuantity;
                    if (vm.BuyOrderTotal - total > total + order.MaxBuyPrice - vm.BuyOrderTotal)
                        order.BuyQuantity += 1;
                }
                if (vm.SetMinSellOrderTotal && order.MinSellPrice != 0) {
                    order.MinSellQuantity = (int)(vm.MinSellOrderTotal / order.MinSellPrice);
                    if (order.MinSellQuantity == 1) order.MinSellQuantity = 0;
                }

                if (vm.SetMaxSellOrderTotal && order.MinSellPrice != 0) {
                    order.MaxSellQuantity = (int)(vm.MaxSellOrderTotal / order.MinSellPrice);
                    if (order.MaxSellQuantity == 0)
                        order.MaxSellQuantity = 1;
                }
            }
            Orders.NotifyOfPropertyChange();
        }

        public void Handle(AddToOrdersEventArgs e) {
            var orders = new List<Order>();
            foreach (InvType item in e.Items) {
                var order = _dataService.Db.Orders.Create();
                order.InvType = item;
                orders.Add(order);
            }
            _eveMarketService.LoadMarketData(orders, DayLimit);
            Orders.AddRange(orders);
            _eventAggregator.Publish(new OrdersAddedEventArgs(orders));
        }

        public void Handle(GridCellValidationEventArgs eventArgs) {
            string value = eventArgs.Value.ToString();
            InvType item = _dataService.Db.InvTypes.SingleOrDefault(f => f.TypeName == value);
            if (item == null) {
                eventArgs.IsValid = false;
                eventArgs.SetError("Invalid item.");
            } else {
                if (Orders.SingleOrDefault(order => order.TypeId == item.TypeId) != null) {
                    eventArgs.IsValid = false;
                    eventArgs.SetError("Item has already been added.");
                } else {
                    ((Order)eventArgs.Row).InvType = item;
                }
            }
        }

        public void Handle(DeleteOrdersEventArgs message) {
            throw new System.NotImplementedException();
        }

        public void Handle(ViewOrderEventArgs message) {
            var order = Orders.Single(o => o.InvType.TypeId == message.InvType.TypeId);
            FocusedOrder = order;
        }
    }
}