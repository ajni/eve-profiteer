using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Repository;

namespace eZet.EveProfiteer.Services {
    public class ApiKeyEntityService : RepositoryService<ApiKeyEntity> {
        protected ApiKeyEntityService(IRepository<ApiKeyEntity> repository)
            : base(repository) {
        }
    }
}
