using System.Collections.Generic;
using System.Linq;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Repository;

namespace eZet.EveProfiteer.Services {
    public class TransactionService : RepositoryService<Transaction> {

        public TransactionService(IRepository<Transaction> repository)
            : base(repository) {
        }

        public long GetLatestId() {
            return (from t in Repository.All()
                    //where t.Entity.Id == Entity.Id
                    orderby t.TransactionId descending
                    select t.TransactionId).FirstOrDefault();
        }

        public IEnumerable<Transaction> RemoveAll(ApiKeyEntity entity) {
            return Repository.RemoveRange(Repository.All().Where(i => i.ApiKeyEntity.Id == entity.Id));
        }

    }
}
