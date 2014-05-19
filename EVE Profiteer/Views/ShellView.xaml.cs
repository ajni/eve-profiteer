using System.Windows;

namespace eZet.EveProfiteer.Views {
    /// <summary>
    ///     Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView {
        public ShellView() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            DevExpress.Xpf.Core.ThemeManager.ApplicationThemeName = "Office2013";
        }
    }
}