using System;
using Caliburn.Micro;
using DevExpress.Xpf.Grid;
using GridControl = DevExpress.XtraGrid.GridControl;

namespace eZet.EveProfiteer.Views {
    /// <summary>
    ///     Interaction logic for OrderEditorView.xaml
    /// </summary>
    public partial class OrderEditorView {
        private readonly IEventAggregator _eventAggregator;

        public OrderEditorView() {
            _eventAggregator = IoC.Get<IEventAggregator>();
            InitializeComponent();
        }

        private void GridViewBase_OnShowingEditor(object sender, ShowingEditorEventArgs e) {
            var view = sender as SelectionView;
            if (Orders.CurrentColumn.FieldName == "ItemName") {
                if (Orders.View.FocusedRowHandle != GridControl.NewItemRowHandle) {
                    e.Cancel = true;
                }
            }
        }

        private void TableView_OnInitNewRow(object sender, InitNewRowEventArgs e) {
            //throw new System.NotImplementedException();
        }

        private void GridViewBase_OnValidateRow(object sender, GridRowValidationEventArgs e) {
            _eventAggregator.Publish(e);
        }

        private void GridViewBase_OnInvalidRowException(object sender, InvalidRowExceptionEventArgs e) {
            throw new NotImplementedException();
        }

        private void GridColumn_OnValidate(object sender, GridCellValidationEventArgs e) {
            _eventAggregator.Publish(e);
        }
    }
}