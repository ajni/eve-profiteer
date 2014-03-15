using System.Collections.Generic;
using eZet.EveLib.EveOnline;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Services {

    public class EveApiService {

        public long KeyId = 3053778;
        public string VCode = "Hu3uslqNc3HDP8XmMMt1Cgb56TpPqqnF2tXssniROFkIMEDLztLPD8ktx6q5WVC2";

        public IList<ApiKeyEntity> GetEntities(long keyId, string vCode) {
            var key = new CharacterKey(KeyId, VCode);
            var list = new List<ApiKeyEntity>();
            foreach (var c in key.Characters) {
                list.Add(new ApiKeyEntity(c.CharacterName, c.CharacterId, "", ""));
            }
            return list;
        }

        
    }
}
