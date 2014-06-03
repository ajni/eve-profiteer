using DevExpress.Data;
using DevExpress.Xpf.Grid;
using eZet.EveProfiteer.Models;
using GridControl = DevExpress.XtraGrid.GridControl;

namespace eZet.EveProfiteer.Views.Tabs {
    /// <summary>
    ///     Interaction logic for OrderEditorView.xaml
    /// </summary>
    public partial class OrderEditorView {

        private int _buyOrders;
        private int _sellOrders;

        public OrderEditorView() {
            InitializeComponent();
        }

        private void GridViewBase_OnShowingEditor(object sender, ShowingEditorEventArgs e) {
            if (OrderEditorGrid.CurrentColumn.FieldName == "TypeName") {
                if (OrderEditorGrid.View.FocusedRowHandle != GridControl.NewItemRowHandle) {
                    e.Cancel = true;
                }
            }
        }

        private void Orders_OnCustomSummary(object sender, CustomSummaryEventArgs e) {
            if (((GridSummaryItem)e.Item).FieldName == "IsSellOrder") {
                if (e.SummaryProcess == CustomSummaryProcess.Start) {
                    _sellOrders = 0;
                } else if (e.SummaryProcess == CustomSummaryProcess.Calculate && (bool)e.FieldValue) {
                    ++_sellOrders;
                    e.TotalValue = _sellOrders;
                }
            }
            if (((GridSummaryItem)e.Item).FieldName == "IsBuyOrder") {
                if (e.SummaryProcess == CustomSummaryProcess.Start) {
                    _buyOrders = 0;
                } else if (e.SummaryProcess == CustomSummaryProcess.Calculate && (bool)e.FieldValue) {
                    ++_buyOrders;
                    e.TotalValue = _buyOrders;
                }
            }
        }

        private void OrdersView_OnInvalidRowException(object sender, InvalidRowExceptionEventArgs e) {
            e.ExceptionMode = ExceptionMode.NoAction;
        }

        private void OrdersView_OnValidateRow(object sender, GridRowValidationEventArgs e) {
            var order = (OrderGridRow)e.Row;
            if (order.TypeId == 0)
                e.IsValid = false;
        }
    }
}