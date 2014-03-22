using System;

namespace eZet.EveProfiteer.ViewModels {
    public class ScannerLinkViewModel  {

        public Uri Uri { get; private set; }

        public ScannerLinkViewModel(Uri uri) {
            Uri = uri;

        }



    }
}
