using System;

namespace eZet.EveProfiteer.Ui.Events {
    public abstract class ModuleEvent {

        public abstract Type GetTabType();

        public bool Focus { get; protected set; }
    }

}
