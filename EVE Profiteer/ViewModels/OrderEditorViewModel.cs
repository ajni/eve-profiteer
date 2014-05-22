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
using eZet.EveProfiteer.Views;
using Screen = Caliburn.Micro.Screen;

namespace eZet.EveProfiteer.ViewModels {
    public class OrderEditorViewModel : Screen, IHandle<AddToOrdersEventArgs>, IHandle<GridCellValidationEventArgs>, IHandle<DeleteOrdersEventArgs> {
        private readonly EveProfiteerDataService _dataService;
        private readonly IEventAggregator _eventAggregator;
        private readonly OrderIoService _orderIoService;
        private readonly IWindowManager _windowManager;


        private BindableCollection<Order> _orders;

        private ObservableCollection<Order> _selectedOrders;
        private string _selectedPath = @"C:\Users\Lars Kristian\AppData\Local\MacroLab\Eve Pilot\Client_1\EVETrader";

        public OrderEditorViewModel(IWindowManager windowManager, IEventAggregator eventAggregator,
            OrderIoService orderIoService, EveProfiteerDataService dataService) {
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _orderIoService = orderIoService;
            _dataService = dataService;
            DisplayName = "Order Editor";
            _eventAggregator.Subscribe(this);

            SelectedOrders = new ObservableCollection<Order>();
            Orders = new BindableCollection<Order>();
            DayLimit = 10;
            BuyOrderAvgOffset = 2;
            SellOrderAvgOffset = 2;
            ViewTradeDetailsCommand = new DelegateCommand<Order>(order => _eventAggregator.Publish(new ViewTradeDetailsEventArgs(order.TypeId)));
            DeleteOrdersCommand = new DelegateCommand<ICollection<object>>(DeleteOrders) ;
        }

        private void DeleteOrders(ICollection<object> collection) {
            var orders = collection.Select(order => (Order) order).ToList();
            foreach (var order in orders) {
                Orders.Remove(order);
            }
            _dataService.Db.Orders.RemoveRange(orders);
            _dataService.Db.SaveChanges();
        }

        public ICommand ViewTradeDetailsCommand { get; private set; }

        public ICommand DeleteOrdersCommand { get; private set; }

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

        public ObservableCollection<Order> SelectedOrders {
            get { return _selectedOrders; }
            set {
                _selectedOrders = value;
                NotifyOfPropertyChange(() => SelectedOrders);
            }
        }

        private void OrdersOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null)
                _dataService.Db.Orders.AddRange(e.NewItems.OfType<Order>());
            if (e.OldItems != null)
                _dataService.Db.Orders.RemoveRange(e.OldItems.OfType<Order>());
        }


        protected override void OnInitialize() {
            Orders.AddRange(_dataService.Db.Orders.Where(order => order.ApiKeyEntity_Id == ShellViewModel.ActiveKeyEntity.Id).ToList());
            Orders.CollectionChanged += OrdersOnCollectionChanged;
        }

        public void SaveChanges() {
            Orders.Apply(order => order.ApiKeyEntity_Id = ShellViewModel.ActiveKeyEntity.Id);
            _dataService.Db.SaveChanges();
        }

        public void UpdateMarketData() {
            _orderIoService.LoadMarketData(Orders, DayLimit);
            Orders.NotifyOfPropertyChange();
            //refreshOrders();
        }

        public void Import() {
            var dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = false;
            dialog.SelectedPath = _selectedPath;
            if (dialog.ShowDialog() == DialogResult.OK) {
                ICollection<Order> orders = _orderIoService.LoadOrdersFromDisk(dialog.SelectedPath);
                _orderIoService.LoadMarketData(orders, DayLimit);
                Orders.Clear();
                Orders.AddRange(orders);
                _selectedPath = dialog.SelectedPath;
            }
        }

        public void Export() {
            var dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = false;
            dialog.SelectedPath = _selectedPath;
            if (dialog.ShowDialog() == DialogResult.OK) {
                _orderIoService.SaveOrdersToDisk(dialog.SelectedPath, Orders);
                _selectedPath = dialog.SelectedPath;
            }
        }

        public void SetPriceLimits() {
            foreach (Order order in SelectedOrders) {
                order.MaxBuyPrice = order.AvgPrice + order.AvgPrice * (decimal)(BuyOrderAvgOffset / 100.0);
                order.MinSellPrice = order.AvgPrice - order.AvgPrice * (decimal)(SellOrderAvgOffset / 100.0);
            }
            refreshOrders();
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
                    var total = order.MaxBuyPrice*order.BuyQuantity;
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
            refreshOrders();
        }

        public void Handle(AddToOrdersEventArgs e) {
            var orders = new List<Order>();
            foreach (MarketAnalyzerEntry item in e.Items) {
                var order = _dataService.Db.Orders.Create();
                order.InvType = item.InvType;
                orders.Add(order);
            }
            _orderIoService.LoadMarketData(orders, DayLimit);
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
                    ((Order)eventArgs.Row).TypeId = item.TypeId;
                }
            }
        }

        private void refreshOrders() {
            var view = GetView() as OrderEditorView;
            if (view != null)
                view.Orders.RefreshData();
        }

        public void Handle(DeleteOrdersEventArgs message) {
            throw new System.NotImplementedException();
        }
    }
}