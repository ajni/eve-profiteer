using Caliburn.Micro;
using DevExpress.Data;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using GridControl = DevExpress.XtraGrid.GridControl;

namespace eZet.EveProfiteer.Views {
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
            var view = sender as SelectionView;
            if (OrderEditorGrid.CurrentColumn.FieldName == "InvType.TypeName") {
                if (OrderEditorGrid.View.FocusedRowHandle != GridControl.NewItemRowHandle) {
                    e.Cancel = true;
                }
            }
        }

        private void Orders_OnCustomSummary(object sender, CustomSummaryEventArgs e) {
            if (((GridSummaryItem) e.Item).FieldName == "IsSellOrder") {
                if (e.SummaryProcess == CustomSummaryProcess.Start) {
                    _sellOrders = 0;
                }
                else if (e.SummaryProcess == CustomSummaryProcess.Calculate && (bool) e.FieldValue) {
                    ++_sellOrders;
                    e.TotalValue = _sellOrders;
                }
            }
            if (((GridSummaryItem) e.Item).FieldName == "IsBuyOrder") {
                if (e.SummaryProcess == CustomSummaryProcess.Start) {
                    _buyOrders = 0;
                }
                else if (e.SummaryProcess == CustomSummaryProcess.Calculate && (bool) e.FieldValue) {
                    ++_buyOrders;
                    e.TotalValue = _buyOrders;
                }
            }
        }
    }
}