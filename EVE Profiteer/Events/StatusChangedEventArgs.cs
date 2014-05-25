using System;

namespace eZet.EveProfiteer.Events {
    public class StatusChangedEventArgs : EventArgs {
        public string Status { get; set; }

        public StatusChangedEventArgs(string status) {
            Status = status;
        }
    }
}
