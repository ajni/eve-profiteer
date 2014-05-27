using System.Windows;
using System.Windows.Controls;
using DevExpress.Xpf.Bars;
using eZet.EveProfiteer.ViewModels;

namespace eZet.EveProfiteer.Views {
    /// <summary>
    ///     Interaction logic for TradeSummaryView.xaml
    /// </summary>
    public partial class TradeSummaryView : UserControl {
        public TradeSummaryView() {
            InitializeComponent();
        }

        private void ViewPeriodSelector_OnEditValueChanged(object sender, RoutedEventArgs e) {
            var value = (TradeSummaryViewModel.ViewPeriodEnum) ((BarEditItem) sender).EditValue;
            if (value == TradeSummaryViewModel.ViewPeriodEnum.Period) {
                StartDate.IsEnabled = true;
                EndDate.IsEnabled = true;
            }
            else if (value == TradeSummaryViewModel.ViewPeriodEnum.Since) {
                StartDate.IsEnabled = true;
                EndDate.IsEnabled = false;
            }
            else {
                StartDate.IsEnabled = false;
                EndDate.IsEnabled = false;
            }
        }
    }
}