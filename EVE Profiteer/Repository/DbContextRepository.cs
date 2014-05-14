using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace eZet.EveProfiteer.Repository {
    public class DbContextRepository<T, TContext> : IRepository<T> where T : class where TContext : DbContext {
        public DbContextRepository(TContext dbContext) {
            DbContext = dbContext;
        }

        public TContext DbContext { get; private set; }

        private DbSet<T> Set {
            get { return DbContext.Set<T>(); }
        }

        public T Create() {
            return Set.Create();
        }

        public bool Validate { get; set; }

        public IQueryable<T> All() {
            return Set;
        }

        public T Add(T entity) {
            return Set.Add(entity);
        }

        public IEnumerable<T> AddRange(IEnumerable<T> entities) {
            return Set.AddRange(entities);
        }

        public T Remove(T entity) {
            return Set.Remove(entity);
        }

        public IEnumerable<T> RemoveRange(IEnumerable<T> entities) {
            return Set.RemoveRange(entities);
        }

        public int SaveChanges() {
            return DbContext.SaveChanges();
        }

        public T Find(int pkey) {
            return Set.Find(pkey);
        }

        public void Dispose() {
            DbContext.Dispose();
        }
    }
}