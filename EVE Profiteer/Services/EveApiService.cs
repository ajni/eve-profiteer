using System;
using System.Collections.Generic;
using System.Linq;
using eZet.EveLib.Modules;
using eZet.EveLib.Modules.Models;
using eZet.EveLib.Modules.Models.Character;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;
using ApiKey = eZet.EveProfiteer.Models.ApiKey;

namespace eZet.EveProfiteer.Services {
    public class EveApiService {

        public CharacterData GetCharacterData(ApiKey key, ApiKeyEntity entity) {
            var data = new CharacterData();
            var ckey = new CharacterKey(key.ApiKeyId, key.VCode);
            var character = ckey.Characters.Single(c => c.CharacterId == entity.EntityId);
            var info  = character.GetCharacterInfo();


            return data;
        }

        public string GetPortraint(ApiKeyEntity entity) {
            return new Image().GetCharacterPortrait(entity.EntityId, Image.CharacterPortraitSize.X256);
        }



        public IList<ApiKeyEntity> GetApiKeyEntities(ApiKey key) {
            var ckey = new CharacterKey(key.ApiKeyId, key.VCode);
            var list = new List<ApiKeyEntity>();
            foreach (Character c in ckey.Characters) {
                list.Add(new ApiKeyEntity { EntityId = c.CharacterId, Name = c.CharacterName, Type = "Character" });
            }
            return list;
        }

        public IEnumerable<Transaction> GetNewTransactions(ApiKey key, ApiKeyEntity entity, long latestId) {
            return getTransactions(key, entity, 2000, latestId);
        }

        public IEnumerable<Transaction> GetAllTransactions(ApiKey key, ApiKeyEntity entity,
            Func<Transaction> transactionFactory) {
            return getTransactions(key, entity, 5000);
        }

        public IList<JournalEntry> GetNewJournalEntries(ApiKey key, ApiKeyEntity entity, long latestId) {
            return getJournalEntries(key, entity, 100, latestId);
        }

        public IList<JournalEntry> GetAllJournalEntries(ApiKey key, ApiKeyEntity entity,
            Func<JournalEntry> transactionFactory) {
            return getJournalEntries(key, entity, 5000);
        }

        private static IList<JournalEntry> getJournalEntries(ApiKey key, ApiKeyEntity entity, int rowLimit,
            long latestId = 0) {
            var list = new List<JournalEntry>();
            var ckey = new CharacterKey(key.ApiKeyId, key.VCode);
            EveApiResponse<WalletJournal> res =
                ckey.Characters.Single(c => c.CharacterId == entity.EntityId).GetWalletJournal(rowLimit);
            IEnumerable<WalletJournal.JournalEntry> transactions = res.Result.Journal.Where(f => f.RefId > latestId);
            IList<WalletJournal.JournalEntry> enumerable = transactions as IList<WalletJournal.JournalEntry> ??
                                                           transactions.ToList();
            int count;
            do {
                count = res.Result.Journal.Count();
                foreach (WalletJournal.JournalEntry t in enumerable) {
                    var transaction = new JournalEntry();
                    transaction.ApiKeyEntity = entity;
                    list.Add(ApiEntityMapper.Map(t, transaction));
                }
                res = res.Result.GetOlder(rowLimit);
            } while (res.Result.Journal.Count() != 0 && enumerable.Count() == count);
            return list;
        }

        private static IEnumerable<Transaction> getTransactions(ApiKey key, ApiKeyEntity apiKeyEntity, int rowLimit,
    long limitId = 0) {
            var transactions = new List<Transaction>();
            var ckey = new CharacterKey(key.ApiKeyId, key.VCode);
            var entity = ckey.Characters.Single(c => c.CharacterId == apiKeyEntity.EntityId);
            EveApiResponse<WalletTransactions> res = entity.GetWalletTransactions(rowLimit);
            while (res.Result.Transactions.Count > 0) {
                var sortedList = res.Result.Transactions.OrderByDescending(f => f.TransactionId);
                foreach (var transaction in sortedList) {
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