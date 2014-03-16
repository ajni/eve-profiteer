using System.Collections.Generic;
using eZet.EveLib.EveOnline;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Services {

    public class EveApiService {

        public IList<ApiKeyEntity> GetEntities(long keyId, string vCode) {
            var key = new CharacterKey(keyId, vCode);
            var list = new List<ApiKeyEntity>();
            foreach (var c in key.Characters) {
                list.Add(new ApiKeyEntity { Id = c.CharacterId, Name = c.CharacterName, Type = "Character" });
            }
            return list;
        }


    }
}
