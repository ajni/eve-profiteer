using DevExpress.Data;
using DevExpress.Xpf.Grid;
using eZet.EveProfiteer.Models;
using GridControl = DevExpress.XtraGrid.GridControl;

namespace eZet.EveProfiteer.Views.Modules {
    /// <summary>
    ///     Interaction logic for OrderManagerView.xaml
    /// </summary>
    public partial class OrderManagerView {

        private int _buyOrders;
        private int _sellOrders;

        public OrderManagerView() {
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
            var order = (OrderViewModel)e.Row;
            if (order.Order.TypeId == 0)
                e.IsValid = false;
        }
    }
}