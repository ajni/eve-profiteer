using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DevExpress.Xpf.Bars;
using eZet.EveProfiteer.ViewModels.Tabs;

namespace eZet.EveProfiteer.Views.Tabs {
    /// <summary>
    /// Interaction logic for TransactionsView.xaml
    /// </summary>
    public partial class TransactionsView : UserControl {
        public TransactionsView() {
            InitializeComponent();
        }

        private void ViewPeriodSelector_OnEditValueChanged(object sender, RoutedEventArgs e) {
            var value = (TransactionsViewModel.ViewPeriodEnum)((BarEditItem)sender).EditValue;
            if (value == TransactionsViewModel.ViewPeriodEnum.Period) {
                StartDate.IsEnabled = true;
                EndDate.IsEnabled = true;
            } else if (value == TransactionsViewModel.ViewPeriodEnum.Since) {
                StartDate.IsEnabled = true;
                EndDate.IsEnabled = false;
            } else {
                StartDate.IsEnabled = false;
                EndDate.IsEnabled = false;
            }
        }
    }
}
