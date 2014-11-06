using System.Linq;
using System.Windows;
using DevExpress.Data;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Views.Modules {
    /// <summary>
    ///     Interaction logic for MarketAnalyzerView.xaml
    /// </summary>
    public partial class MarketAnalyzerView {
        public MarketAnalyzerView() {
            InitializeComponent();
        }


        private void AddCustomColumnButton_OnItemClick(object sender, ItemClickEventArgs e) {
            var col = new GridColumn();
            col.Header = "Custom";
            col.AllowUnboundExpressionEditor = true;
            col.UnboundType = UnboundColumnType.Decimal;
            MarketAnalyzerGrid.Columns.Add(col);
        }

        private void MarketAnalyzerGrid_OnCopyingToClipboard(object sender, CopyingToClipboardEventArgs e) {
            if (e.RowHandles.Count() == 1) {
                var row = (MarketAnalyzerEntry)e.Source.DataControl.GetRow(e.RowHandles.Single());
                Clipboard.SetText(row.InvType.TypeName);
                e.Handled = true;
            }
        }
    }
}