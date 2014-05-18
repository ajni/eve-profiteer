using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using Caliburn.Micro;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Mvvm;
using DevExpress.Xpf.Ribbon.Customization;
using eZet.EveOnlineDbModels;
using eZet.EveProfiteer.Events;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;
using eZet.EveProfiteer.Views;
using Screen = Caliburn.Micro.Screen;

namespace eZet.EveProfiteer.ViewModels {
    public class OrderEditorViewModel : Screen, IHandle<AddToOrdersEventArgs>, IHandle<GridCellValidationEventArgs> {
        private readonly EveOnlineStaticDataService _eveOnlineStaticDataService;
        private readonly IEventAggregator _eventAggregator;
        private readonly OrderEditorService _orderEditorService;
        private readonly IWindowManager _windowManager;


        private ObservableCollection<Order> _orders;

        private ObservableCollection<Order> _selectedOrders;
        private string _selectedPath = @"C:\Users\Lars Kristian\AppData\Local\MacroLab\Eve Pilot\Client_1\EVETrader";

        public OrderEditorViewModel(IWindowManager windowManager, IEventAggregator eventAggregator,
            OrderEditorService orderEditorService, EveOnlineStaticDataService eveOnlineStaticDataService) {
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _orderEditorService = orderEditorService;
            _eveOnlineStaticDataService = eveOnlineStaticDataService;
            DisplayName = "Order Editor";

            _eventAggregator.Subscribe(this);

            SelectedOrders = new ObservableCollection<Order>();
            Orders = new ObservableCollection<Order>();
            DayLimit = 10;
            BuyOrderAvgOffset = 2;
            SellOrderAvgOffset = 2;
            ViewTradeDetailsCommand = new DelegateCommand<Order>(order => _eventAggregator.Publish(new ViewTradeDetailsEventArgs(order.TypeId)));}

        public ICommand ViewTradeDetailsCommand { get; private set; }

        public int DayLimit { get; set; }

        public int BuyOrderAvgOffset { get; set; }

        public int SellOrderAvgOffset { get; set; }

        public ObservableCollection<Order> Orders {
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
                _orderEditorService.AddOrders(e.NewItems.OfType<Order>());
            if (e.OldItems != null)
                _orderEditorService.DeleteOrders(e.OldItems.OfType<Order>());
        }


        protected override void OnInitialize() {
            Orders.AddRange(_orderEditorService.GetOrders().ToList());
            Orders.CollectionChanged += OrdersOnCollectionChanged;
        }

        public void SaveChanges() {
            _orderEditorService.SaveChanges();
        }

        public void UpdateMarketData() {
            _orderEditorService.LoadMarketData(Orders, DayLimit);
            refreshOrders();
        }

        public void Import() {
            var dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = false;
            dialog.SelectedPath = _selectedPath;
            if (dialog.ShowDialog() == DialogResult.OK) {
                ICollection<Order> orders = _orderEditorService.LoadOrdersFromDisk(dialog.SelectedPath);
                _orderEditorService.LoadMarketData(orders, DayLimit);
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
                _orderEditorService.SaveOrdersToDisk(dialog.SelectedPath, Orders);
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
                    if (vm.BuyOrderTotal <= order.MaxBuyPrice)
                        order.BuyQuantity = 1;
                }
                if (vm.SetMinSellOrderTotal && order.MinSellPrice != 0) {
                    order.MinSellQuantity = (int)(vm.MinSellOrderTotal / order.MinSellPrice);
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
                orders.Add(_orderEditorService.CreateOrder(item.InvType.TypeId));
            }
            _orderEditorService.LoadMarketData(orders, DayLimit);
            Orders.AddRange(orders);
            _eventAggregator.Publish(new OrdersAddedEventArgs(orders));
        }

        public void Handle(GridCellValidationEventArgs eventArgs) {
            string value = eventArgs.Value.ToString();
            InvType item = _eveOnlineStaticDataService.GetTypes().SingleOrDefault(f => f.TypeName == value);
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
    }
}