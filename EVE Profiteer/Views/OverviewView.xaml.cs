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
using eZet.EveProfiteer.ViewModels;

namespace eZet.EveProfiteer.Views {
    /// <summary>
    /// Interaction logic for OverviewView.xaml
    /// </summary>
    public partial class OverviewView : UserControl {
        public OverviewView() {
            InitializeComponent();
        }

        private void ViewPeriodSelector_OnEditValueChanged(object sender, RoutedEventArgs e) {
            var value = (OverviewViewModel.ViewPeriodEnum)((BarEditItem)sender).EditValue;
            if (value == OverviewViewModel.ViewPeriodEnum.Period) {
                StartDate.IsEnabled = true;
                EndDate.IsEnabled = true;
            } else if (value == OverviewViewModel.ViewPeriodEnum.Since) {
                StartDate.IsEnabled = true;
                EndDate.IsEnabled = false;
            } else {
                StartDate.IsEnabled = false;
                EndDate.IsEnabled = false;
            }
        }
    }
}
