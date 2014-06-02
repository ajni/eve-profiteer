using DevExpress.Data;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;

namespace eZet.EveProfiteer.Views.Tabs {
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
    }
}