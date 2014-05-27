using System.Collections.Generic;
using System.Linq;
using eZet.EveProfiteer.Repository;

namespace eZet.EveProfiteer.Services {
    public class RepositoryService<T> where T : class {
        protected readonly IRepository<T> Db;

        public RepositoryService(IRepository<T> db) {
            Db = db;
        }

        public bool Validate {
            get { return Db.Validate; }
            set { Db.Validate = value; }
        }

        public void Dispose() {
            Db.Dispose();
        }

        public T Create() {
            return Db.Create();
        }

        public IQueryable<T> Transactions() {
            return Db.Queryable();
        }

        public T Add(T entity) {
            return Db.Add(entity);
        }

        public IEnumerable<T> AddRange(IEnumerable<T> entities) {
            return Db.AddRange(entities);
        }

        public T Remove(T entity) {
            return Db.Remove(entity);
        }

        public IEnumerable<T> RemoveRange(IEnumerable<T> entities) {
            return Db.RemoveRange(entities);
        }

        public int SaveChanges() {
            return Db.SaveChanges();
        }

        public T Find(int pkey) {
            return Db.Find(pkey);
        }
    }
}