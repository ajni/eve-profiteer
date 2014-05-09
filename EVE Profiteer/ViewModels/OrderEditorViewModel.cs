using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using Caliburn.Micro;
using DevExpress.Xpf.Grid;
using DevExpress.XtraEditors.DXErrorProvider;
using eZet.Eve.OrderIoHelper.Models;
using eZet.EveProfiteer.Services;
using eZet.EveProfiteer.Views;
using Screen = Caliburn.Micro.Screen;

namespace eZet.EveProfiteer.ViewModels {
    public class OrderEditorViewModel : Screen, IHandle<object> {
        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly OrderEditorService _orderEditorService;
        private readonly EveDataService _eveDataService;

        private string _selectedPath = @"C:\Users\Lars Kristian\AppData\Local\MacroLab\Eve Pilot\Client_1\EVETrader";


        private ObservableCollection<Order> _orders;

        private ObservableCollection<Order> _selectedOrders;

        public int DayLimit { get; set; }

        public int BuyOrderAvgOffset { get; set; }

        public int SellOrderAvgOffset { get; set; }

        public ObservableCollection<Order> Orders {
            get { return _orders; }
            private set { _orders = value; NotifyOfPropertyChange(() => Orders); }
        }

        public ObservableCollection<Order> SelectedOrders {
            get { return _selectedOrders; }
            set { _selectedOrders = value; NotifyOfPropertyChange(() => SelectedOrders); }
        }


        public OrderEditorViewModel(IWindowManager windowManager, IEventAggregator eventAggregator, OrderEditorService orderEditorService, EveDataService eveDataService) {
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _orderEditorService = orderEditorService;
            _eveDataService = eveDataService;
            DisplayName = "Order Editor";


            _eventAggregator.Subscribe(this);

            SelectedOrders = new ObservableCollection<Order>();
            Orders = new ObservableCollection<Order>();
            DayLimit = 10;
            BuyOrderAvgOffset = 2;
            SellOrderAvgOffset = 2;
        }

        public void Open() {
            var dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = false;
            dialog.SelectedPath = _selectedPath;
            if (dialog.ShowDialog() == DialogResult.OK) {
                var orders = _orderEditorService.LoadOrders(dialog.SelectedPath);
                _orderEditorService.LoadPriceData(orders, DayLimit);
                Orders = new ObservableCollection<Order>(orders);
                _selectedPath = dialog.SelectedPath;
            }
        }

        public void Save() {
            var dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = false;
            dialog.SelectedPath = _selectedPath;
            if (dialog.ShowDialog() == DialogResult.OK) {
                _orderEditorService.SaveOrders(dialog.SelectedPath, Orders);
                _selectedPath = dialog.SelectedPath;
            }
        }

        public void Average() {
            foreach (var order in SelectedOrders) {
                order.MaxBuyPrice = order.AvgPrice + order.AvgPrice * (decimal)(BuyOrderAvgOffset / 100.0);
                order.MinSellPrice = order.AvgPrice - order.AvgPrice * (decimal)(SellOrderAvgOffset / 100.0);
            }
            ((OrderEditorView)GetView()).Orders.RefreshData();
        }

        public void SelectAll() {
            var list = new List<Order>();
            foreach (var order in Orders)
                list.Add(order);
            SelectedOrders = new ObservableCollection<Order>(list);
        }

        public void SelectNone() {
            SelectedOrders.Clear();
        }

        public void EditOrdersDialog() {
            var vm = new EditOrderDialogViewModel();
            if (_windowManager.ShowDialog(vm) != true) return;
            foreach (var order in SelectedOrders) {
                if (vm.SetBuyOrderTotal && order.MaxBuyPrice != 0) {
                    order.BuyQuantity = (int)(vm.BuyOrderTotal / order.MaxBuyPrice);
                    if (order.BuyQuantity == 0)
                        order.BuyQuantity = 1;
                }
                if (vm.SetMinSellOrderTotal && order.MinSellPrice != 0) {
                    order.MinSellQuantity = (int) (vm.MinSellOrderTotal/order.MinSellPrice);
                    if (order.MinSellQuantity == 0)
                        order.MinSellQuantity = 1;
                }

                if (vm.SetMaxSellOrderTotal && order.MinSellPrice != 0) {
                    order.MaxSellQuantity = (int)(vm.MaxSellOrderTotal / order.MinSellPrice);
                    if (order.MaxSellQuantity == 0)
                        order.MaxSellQuantity = 1;
                }
            }
            ((OrderEditorView)GetView()).Orders.RefreshData();
        }

        public void Handle(object message) {
            if (message.GetType() != typeof (GridCellValidationEventArgs)) return;
            var e = message as GridCellValidationEventArgs;
            var value = e.Value.ToString();
            var item = _eveDataService.GetItems().SingleOrDefault(f => f.TypeName == value);
            if (item == null) {
                e.IsValid = false;
                e.SetError("Invalid item.");
            } else {
                if (Orders.SingleOrDefault(order => order.ItemId == item.TypeId) != null) {
                    e.IsValid = false;
                    e.SetError("Item has already been added.");
                }
                else {
                    ((Order) e.Row).ItemId = item.TypeId;
                }
            }
        }
    }
}
