using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Xpf.Grid;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.ViewModels;

namespace eZet.EveProfiteer.Views {
    /// <summary>
    /// Interaction logic for TradeAnalyzerView.xaml
    /// </summary>
    public partial class TradeAnalyzerView : UserControl {
        public TradeAnalyzerView() {
            InitializeComponent();
        }

        private void TradeAnalyzerGrid_OnCustomRowFilter(object sender, RowFilterEventArgs e) {
            int rowHandle = TradeAnalyzerGrid.GetRowHandleByListIndex(e.ListSourceRowIndex);
            var row = e.Source.GetRow(rowHandle) as TradeAnalyzerItem;
            if (row == null)
                throw new InvalidOperationException();
            if (FilterOrders.IsChecked.Value && row.Order == null)e.Visible = false;
            if (FilterInactiveOrders.IsChecked.Value && (row.Order == null || row.Order.BuyQuantity == 0)) e.Visible = false;

            e.Handled = !e.Visible;
        }

        private void FilterOrders_OnChecked(object sender, RoutedEventArgs e) {
            TradeAnalyzerGrid.RefreshData();
        }
    }
}
