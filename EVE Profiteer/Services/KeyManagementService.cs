using System.Collections.Generic;
using System.Linq;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Repository;

namespace eZet.EveProfiteer.Services {
    public class KeyManagementService {

        private IRepository<ApiKey> ApiKeyRepository { get; set; }

        private IRepository<ApiKeyEntity> ApiKeyEntityRepository { get; set; }

        public KeyManagementService(IRepository<ApiKey> apiKeyRepository, IRepository<ApiKeyEntity> apiKeyEntityRepository) {
            ApiKeyRepository = apiKeyRepository;
            ApiKeyEntityRepository = apiKeyEntityRepository;
        }

        public ApiKey AddKey(ApiKey key, IEnumerable<ApiKeyEntity> entities) {
            foreach (var c in entities) {
                var entity = ApiKeyEntityRepository.All().SingleOrDefault(e => e.EntityId == c.EntityId);
                if (entity != null) {
                    entity.IsActive = c.IsActive;
                } else {
                    entity = c;
                    ApiKeyEntityRepository.Add(entity);
                }
                entity.ApiKeys.Add(key);
                key.Entities.Add(entity);
            }
            ApiKeyRepository.SaveChanges();
            ApiKeyEntityRepository.SaveChanges();
            return key;
        }

        public void DeleteKey(ApiKey key) {
            ApiKeyRepository.Remove(key);
            ApiKeyRepository.SaveChanges();
        }

        public IQueryable<ApiKey> AllApiKeys() {
            return ApiKeyRepository.All();
        }

        public ApiKey CreateApiKey() {
            return ApiKeyRepository.Create();
        }

        public ApiKeyEntity CreateApiKeyEntity() {
            return ApiKeyEntityRepository.Create();
        }
    }
}
