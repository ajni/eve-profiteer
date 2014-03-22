using System;
using System.Windows;

namespace eZet.Eve.EveProfiteer.Views {
    /// <summary>
    /// Interaction logic for ScannerLinkView.xaml
    /// </summary>
    public partial class ScannerLinkView : Window {
        public ScannerLinkView() {
            InitializeComponent();
        }

        private void Copy_OnClick(object sender, RoutedEventArgs e) {
            Clipboard.SetText(Link.Text);
        }
    }
}
