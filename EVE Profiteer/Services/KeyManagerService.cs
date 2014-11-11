using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Services {
    public class KeyManagerService : DbContextService {
        public KeyManagerService(EveProfiteerRepository repository) {
            this.repository = repository;
        }

        private readonly EveProfiteerRepository repository;

        public Task AddKey(ApiKey key) {
            repository.Context.ApiKeys.Add(key);
            return repository.Context.SaveChangesAsync();
        }

        public Task DeleteKey(ApiKey key) {
            repository.Context.ApiKeys.Remove(key);
            return repository.Context.SaveChangesAsync();
        }

        public Task<List<ApiKey>> GetKeys() {
            return repository.Context.ApiKeys.Include(e => e.ApiKeyEntities).ToListAsync();
        }

        public Task Update(ApiKey key, ICollection<ApiKeyEntity> entities) {
            foreach (var entity in entities) {
                var existing = key.ApiKeyEntities.SingleOrDefault(e => e.EntityId == entity.EntityId);
                if (existing != null) {
                    entity.Id = existing.Id;
                    key.ApiKeyEntities.Remove(existing);
                }
                key.ApiKeyEntities.Add(entity);
            }
            foreach (var entity in key.ApiKeyEntities.Where(entity => entities.All(e => e.EntityId != entity.EntityId))) {
                key.ApiKeyEntities.Remove(entity);
            }
            return repository.SaveChangesAsync();
        }
    }
}
