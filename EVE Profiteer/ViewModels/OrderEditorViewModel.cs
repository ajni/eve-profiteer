using System.Windows.Forms;
using eZet.Eve.OrderIoHelper.Models;
using eZet.EveProfiteer.Services;
using eZet.EveProfiteer.Views;
using Screen = Caliburn.Micro.Screen;

namespace eZet.EveProfiteer.ViewModels {
    public class OrderEditorViewModel : Screen {

        private readonly OrderEditorService _orderEditorService;

        private string _selectedPath = @"C:\Users\Lars Kristian\AppData\Local\MacroLab\Eve Pilot\Client_1\EVETrader";


        private OrderCollection _orders;

        public int DayLimit { get; set; }

        public int BuyOrderAvgOffset { get; set; }

        public int SellOrderAvgOffset { get; set; }

        public OrderCollection Orders {
            get { return _orders; }
            private set { _orders = value; NotifyOfPropertyChange(() => Orders); }
        }


        public OrderEditorViewModel(OrderEditorService orderEditorService) {
            _orderEditorService = orderEditorService;
            DisplayName = "Order Editor";
            
            DayLimit = 10;
            BuyOrderAvgOffset = 2;
            SellOrderAvgOffset = 2;

            var orders = _orderEditorService.LoadOrders("../../samples");
            _orderEditorService.LoadPriceData(orders, DayLimit);
            Orders = orders;
        }

        public void Open() {
            var dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = false;
            dialog.SelectedPath = _selectedPath;
            if (dialog.ShowDialog() == DialogResult.OK) {
                var orders = _orderEditorService.LoadOrders(dialog.SelectedPath);
                _orderEditorService.LoadPriceData(orders, DayLimit);
                Orders = orders;
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
            foreach (var order in Orders) {
                order.MaxBuyPrice = order.AvgPrice + order.AvgPrice * (decimal)(BuyOrderAvgOffset / 100.0);
                order.MinSellPrice = order.AvgPrice - order.AvgPrice * (decimal)(SellOrderAvgOffset / 100.0);
            }
            ((OrderEditorView)GetView()).Orders.RefreshData();
        }


    }
}
