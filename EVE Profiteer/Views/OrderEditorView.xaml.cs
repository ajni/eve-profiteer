using Caliburn.Micro;
using DevExpress.Data;
using DevExpress.Xpf.Grid;
using GridControl = DevExpress.XtraGrid.GridControl;

namespace eZet.EveProfiteer.Views {
    /// <summary>
    ///     Interaction logic for OrderEditorView.xaml
    /// </summary>
    public partial class OrderEditorView {
        private readonly IEventAggregator _eventAggregator;

        private int buyOrders;
        private int sellOrders;

        public OrderEditorView() {
            _eventAggregator = IoC.Get<IEventAggregator>();
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

        private void TypeName_OnValidate(object sender, GridCellValidationEventArgs e) {
            _eventAggregator.Publish(e);
        }

        private void Orders_OnCustomSummary(object sender, CustomSummaryEventArgs e) {
            if (((GridSummaryItem) e.Item).FieldName == "IsSellOrder") {
                if (e.SummaryProcess == CustomSummaryProcess.Start) {
                    sellOrders = 0;
                }
                else if (e.SummaryProcess == CustomSummaryProcess.Calculate && (bool) e.FieldValue) {
                    ++sellOrders;
                    e.TotalValue = sellOrders;
                }
            }
            if (((GridSummaryItem) e.Item).FieldName == "IsBuyOrder") {
                if (e.SummaryProcess == CustomSummaryProcess.Start) {
                    buyOrders = 0;
                }
                else if (e.SummaryProcess == CustomSummaryProcess.Calculate && (bool) e.FieldValue) {
                    ++buyOrders;
                    e.TotalValue = buyOrders;
                }
            }
        }
    }
}