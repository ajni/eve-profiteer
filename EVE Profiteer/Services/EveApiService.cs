using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using eZet.EveLib.Modules;
using eZet.EveLib.Modules.Models;
using eZet.EveLib.Modules.Models.Character;
using eZet.EveLib.Modules.Models.Misc;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;
using ApiKey = eZet.EveProfiteer.Models.ApiKey;

namespace eZet.EveProfiteer.Services {
    public class EveApiService {
        public CharacterData GetCharacterData(ApiKey key, ApiKeyEntity entity) {
            var data = new CharacterData();
            var ckey = new CharacterKey(key.ApiKeyId, key.VCode);
            Character character = ckey.Characters.Single(c => c.CharacterId == entity.EntityId);
            EveApiResponse<CharacterInfo> info = character.GetCharacterInfo();
            return data;
        }

        public string GetPortraint(ApiKeyEntity entity) {
            return new Image().GetCharacterPortrait(entity.EntityId, Image.CharacterPortraitSize.X256, @"c:\Temp");
        }

        public Task<EveApiResponse<ReferenceTypes>> GetRefTypesAsync() {
            return new EveLib.Modules.Eve().GetReferenceTypesAsync();
        }


        public IList<ApiKeyEntity> GetApiKeyEntities(ApiKey key) {
            var ckey = new CharacterKey(key.ApiKeyId, key.VCode);
            var list = new List<ApiKeyEntity>();
            foreach (Character c in ckey.Characters) {
                list.Add(new ApiKeyEntity { EntityId = c.CharacterId, Name = c.CharacterName, Type = "Character" });
            }
            return list;
        }

        public Task<IList<Transaction>> GetNewTransactionsAsync(ApiKey key, ApiKeyEntity entity, long latestId) {
            return getTransactionsAsync(key, entity, 5000, latestId);
        }

        public Task<IList<Transaction>> GetAllTransactionsAsync(ApiKey key, ApiKeyEntity entity,
            Func<Transaction> transactionFactory) {
            return getTransactionsAsync(key, entity, 5000);
        }

        public Task<IList<JournalEntry>> GetNewJournalEntriesAsync(ApiKey key, ApiKeyEntity entity, long latestId) {
            return getJournalEntries(key, entity, 5000, latestId);
        }

        public Task<IList<JournalEntry>> GetAllJournalEntriesAsync(ApiKey key, ApiKeyEntity entity,
            Func<JournalEntry> transactionFactory) {
            return getJournalEntries(key, entity, 5000);
        }

        public async Task<AssetList> GetAssetsAsync(ApiKey key, ApiKeyEntity entity) {
            var data = new CharacterData();
            var ckey = await new CharacterKey(key.ApiKeyId, key.VCode).InitAsync().ConfigureAwait(false);
            Character character = ckey.Characters.Single(c => c.CharacterId == entity.EntityId);
            var assets = await character.GetAssetListAsync().ConfigureAwait(false);
            return assets.Result;
        }

        private static async Task<IList<JournalEntry>> getJournalEntries(ApiKey key, ApiKeyEntity apiKeyEntity,
            int rowLimit,
            long limitId = 0) {
            var list = new List<JournalEntry>();
            var ckey = await new CharacterKey(key.ApiKeyId, key.VCode).InitAsync().ConfigureAwait(false);
            Character entity = ckey.Characters.Single(c => c.CharacterId == apiKeyEntity.EntityId);
            var res = await entity.GetWalletJournalAsync(rowLimit).ConfigureAwait(false);
            while (res.Result.Journal.Count > 0) {
                var sortedList = res.Result.Journal.OrderByDescending(f => f.RefId);
                foreach (var entry in sortedList) {
                    if (entry.RefId == limitId) {
                        return list;
                    }
                    var newEntry = new JournalEntry();
                    newEntry.ApiKeyEntity_Id = apiKeyEntity.Id;
                    list.Add(ApiEntityMapper.Map(entry, newEntry));
                }
                try {
                    res =
                        await entity.GetWalletJournalAsync(rowLimit, sortedList.Last().RefId).ConfigureAwait(false);
                } catch (Exception e) {
                    return list;
                }
            }
            return list;
        }

        private static async Task<IList<Transaction>> getTransactionsAsync(ApiKey key, ApiKeyEntity apiKeyEntity, int rowLimit,
            long limitId = 0) {
            var transactions = new List<Transaction>();
            var ckey = await new CharacterKey(key.ApiKeyId, key.VCode).InitAsync().ConfigureAwait(false);
            Character entity = ckey.Characters.Single(c => c.CharacterId == apiKeyEntity.EntityId);
            EveApiResponse<WalletTransactions> res = await entity.GetWalletTransactionsAsync(rowLimit).ConfigureAwait(false);
            while (res.Result.Transactions.Count > 0) {
                IOrderedEnumerable<WalletTransactions.Transaction> sortedList =
                    res.Result.Transactions.OrderByDescending(f => f.TransactionId);
                foreach (WalletTransactions.Transaction transaction in sortedList) {
                    if (transaction.TransactionId == limitId)
                        return transactions;
                    var newTransaction = new Transaction();
                    newTransaction.ApiKeyEntity_Id = apiKeyEntity.Id;
                    transactions.Add(ApiEntityMapper.Map(transaction, newTransaction));
                }

                res = entity.GetWalletTransactions(rowLimit, sortedList.Last().TransactionId);
            }
            return transactions;
        }

    }
}