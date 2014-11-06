using System.Windows;
using System.Windows.Controls;

namespace eZet.EveProfiteer.Views.Modules {
    /// <summary>
    /// Interaction logic for TransactionsView.xaml
    /// </summary>
    public partial class TransactionsView : UserControl {
        public TransactionsView() {
            InitializeComponent();
        }

        private void ViewPeriodSelector_OnEditValueChanged(object sender, RoutedEventArgs e) {
            //var value = (TransactionsViewModel.ViewPeriodEnum)((BarEditItem)sender).EditValue;
            //if (value == TransactionsViewModel.ViewPeriodEnum.Period) {
            //    StartDate.IsEnabled = true;
            //    EndDate.IsEnabled = true;
            //} else if (value == TransactionsViewModel.ViewPeriodEnum.Since) {
            //    StartDate.IsEnabled = true;
            //    EndDate.IsEnabled = false;
            //} else {
            //    StartDate.IsEnabled = false;
            //    EndDate.IsEnabled = false;
            //}
        }
    }
}
