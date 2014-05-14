using System;

namespace eZet.EveProfiteer.ViewModels {
    public class ScannerLinkViewModel {
        public ScannerLinkViewModel(Uri uri) {
            Uri = uri;
        }

        public Uri Uri { get; private set; }
    }
}