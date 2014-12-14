using eZet.EveLib.Modules.Models.Account;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Ui.Events {
    public class ActiveEntityChangedEvent {

        public ActiveEntityChangedEvent(ApiKeyEntity newEntity) {
            NewEntity = newEntity;
        }

        public ApiKeyEntity NewEntity { get; private set; }

    }
}
