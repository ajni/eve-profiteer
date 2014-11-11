using System;

namespace eZet.EveProfiteer.Ui.Events {
    public class StatusChangedEventArgs : EventArgs {
        public StatusChangedEventArgs(string status) {
            Status = status;
        }

        public StatusChangedEventArgs(string stringFormat, params object[] args) {
            Status = String.Format(stringFormat, args);
        }

        public string Status { get; set; }
    }
}