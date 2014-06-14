using System.Windows;
using DevExpress.Xpf.Bars;
using eZet.EveProfiteer.ViewModels.Tabs;

namespace eZet.EveProfiteer.Views.Tabs {
    /// <summary>
    ///     Interaction logic for TransactionDetailsView.xaml
    /// </summary>
    public partial class TransactionDetailsView {
        public TransactionDetailsView() {
            InitializeComponent();
        }

        private void ViewPeriodSelector_OnEditValueChanged(object sender, RoutedEventArgs e) {
            var value = (TransactionDetailsViewModel.ViewPeriodEnum)((BarEditItem)sender).EditValue;
            if (value == TransactionDetailsViewModel.ViewPeriodEnum.Period) {
                StartDate.IsEnabled = true;
                EndDate.IsEnabled = true;
            } else if (value == TransactionDetailsViewModel.ViewPeriodEnum.Since) {
                StartDate.IsEnabled = true;
                EndDate.IsEnabled = false;
            } else {
                StartDate.IsEnabled = false;
                EndDate.IsEnabled = false;
            }
        }
    }
}