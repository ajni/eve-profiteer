using System.Windows;
using DevExpress.Xpf.Core;

namespace eZet.EveProfiteer.Views {
    /// <summary>
    ///     Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : Window {
        public ShellView() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            ThemeManager.ApplicationThemeName = "Office2013";
        }
    }
}