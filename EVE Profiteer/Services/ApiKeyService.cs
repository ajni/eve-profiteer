using System.Collections.Generic;
using System.Linq;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Repository;

namespace eZet.EveProfiteer.Services {
    public class ApiKeyService {

        public RepositoryService<ApiKey> ApiKeyRepository { get; private set; }

        public RepositoryService<ApiKeyEntity> ApiKeyEntityRepository { get; private set; }

        public ApiKeyService(RepositoryService<ApiKey> apiKeyRepository, RepositoryService<ApiKeyEntity> apiKeyEntityRepository) {
            ApiKeyRepository = apiKeyRepository;
            ApiKeyEntityRepository = apiKeyEntityRepository;
        }

        public void AddKey(ApiKey key, IEnumerable<ApiKeyEntity> entities) {
            foreach (var c in entities) {
                var entity = ApiKeyEntityRepository.GetAll().SingleOrDefault(e => e.EntityId == c.EntityId);
                if (entity != null) {
                    entity.IsActive = c.IsActive;
                } else {
                    entity = c;
                }
                entity.ApiKeys.Add(key);
                key.Entities.Add(entity);
            }
            ApiKeyRepository.SaveChanges();
            ApiKeyEntityRepository.SaveChanges();
        }


    }
}
