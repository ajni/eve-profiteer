using System.Collections.Generic;
using System.Linq;
using eZet.EveLib.EveOnline;
using eZet.EveLib.EveOnline.Model.Character;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Services {

    public class EveApiService {

        public ICollection<ApiKeyEntity> GetEntities(long keyId, string vCode) {
            var key = new CharacterKey(keyId, vCode);
            var list = new List<ApiKeyEntity>();
            foreach (var c in key.Characters) {
                list.Add(new ApiKeyEntity { EntityId = c.CharacterId, Name = c.CharacterName, Type = "Character" });
            }
            return list;
        }

        public ICollection<WalletTransactions.Transaction> GetAllTransactions(Models.ApiKey key, ApiKeyEntity entity) {
            var ckey = new CharacterKey(key.ApiKeyId, key.VCode);
            var res = ckey.Characters.Single(c => c.CharacterId == entity.EntityId).GetWalletTransactions(5000);
            var list = new List<WalletTransactions.Transaction>(res.Result.Transactions);
            //while (true) {
            //    res = res.Result.GetOlder(10000);
            //    list.AddRange(res.Result.Transactions);
            //}
            return list;
        }


    }
}
