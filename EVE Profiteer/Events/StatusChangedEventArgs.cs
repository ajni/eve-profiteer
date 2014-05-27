using System;

namespace eZet.EveProfiteer.Events {
    public class StatusChangedEventArgs : EventArgs {
        public StatusChangedEventArgs(string status) {
            Status = status;
        }

        public string Status { get; set; }
    }
}