using System.Collections.Generic;
using System.Linq;
using eZet.EveProfiteer.Repository;

namespace eZet.EveProfiteer.Services {
    public class RepositoryService<T> where T : class {
        protected readonly IRepository<T> Repository;

        public RepositoryService(IRepository<T> repository) {
            Repository = repository;
        }

        public bool Validate {
            get { return Repository.Validate; }
            set { Repository.Validate = value; }
        }

        public void Dispose() {
            Repository.Dispose();
        }

        public T Create() {
            return Repository.Create();
        }

        public IQueryable<T> All() {
            return Repository.All();
        }

        public T Add(T entity) {
            return Repository.Add(entity);
        }

        public IEnumerable<T> AddRange(IEnumerable<T> entities) {
            return Repository.AddRange(entities);
        }

        public T Remove(T entity) {
            return Repository.Remove(entity);
        }

        public IEnumerable<T> RemoveRange(IEnumerable<T> entities) {
            return Repository.RemoveRange(entities);
        }

        public int SaveChanges() {
            return Repository.SaveChanges();
        }

        public T Find(int pkey) {
            return Repository.Find(pkey);
        }
    }
}