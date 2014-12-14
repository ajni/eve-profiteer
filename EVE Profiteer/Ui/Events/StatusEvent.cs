using System;
using eZet.EveProfiteer.ViewModels;

namespace eZet.EveProfiteer.Ui.Events {
    public class StatusEvent : EventArgs {
        public StatusEvent(string status) {
            Status = status;
        }


        public StatusEvent(ModuleViewModel module, string status) {
            Status = module.DisplayName + " : " + status;
        }


        public StatusEvent(string stringFormat, params object[] args) {
            Status = String.Format(stringFormat, args);
        }

        public string Status { get; set; }
    }
}