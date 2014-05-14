using System.Windows;
using System.Windows.Controls.Primitives;

namespace eZet.EveProfiteer.Views {
    /// <summary>
    ///     Interaction logic for MarketAnalyzerView.xaml
    /// </summary>
    public partial class MarketAnalyzerView {
        public MarketAnalyzerView() {
            InitializeComponent();
            Splitter.DragDelta += SplitterNameDragDelta;
        }

        private void SplitterNameDragDelta(object sender, DragDeltaEventArgs e) {
            MainGrid.ColumnDefinitions[0].Width =
                new GridLength(MainGrid.ColumnDefinitions[0].ActualWidth + e.HorizontalChange);
        }
    }
}