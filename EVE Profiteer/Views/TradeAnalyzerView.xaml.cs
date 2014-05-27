using System;
using System.Windows;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.ViewModels;

namespace eZet.EveProfiteer.Views {
    /// <summary>
    /// Interaction logic for TradeAnalyzerView.xaml
    /// </summary>
    public partial class TradeAnalyzerView {
        public TradeAnalyzerView() {
            InitializeComponent();
        }

        private void TradeAnalyzerGrid_OnCustomRowFilter(object sender, RowFilterEventArgs e) {
            int rowHandle = TradeAnalyzerGrid.GetRowHandleByListIndex(e.ListSourceRowIndex);
            var row = e.Source.GetRow(rowHandle) as TransactionAggregate;
            if (row == null)
                throw new InvalidOperationException();
            if (FilterOrders.IsChecked.Value && row.Order == null) e.Visible = false;
            if (FilterInactiveOrders.IsChecked.Value && (row.Order == null || !row.Order.IsBuyOrder)) e.Visible = false;

            e.Handled = !e.Visible;
        }

        private void FilterOrders_OnChecked(object sender, RoutedEventArgs e) {
            TradeAnalyzerGrid.RefreshData();
        }

        private void ViewPeriodSelector_OnEditValueChanged(object sender, RoutedEventArgs e) {
            var value = (TradeAnalyzerViewModel.ViewPeriodEnum)((BarEditItem)sender).EditValue;
            if (value == TradeAnalyzerViewModel.ViewPeriodEnum.Period) {
                StartDate.IsEnabled = true;
                EndDate.IsEnabled = true;
            } else if (value == TradeAnalyzerViewModel.ViewPeriodEnum.Since) {
                StartDate.IsEnabled = true;
                EndDate.IsEnabled = false;
            } else {
                StartDate.IsEnabled = false;
                EndDate.IsEnabled = false;
            }
        }
    }
}
