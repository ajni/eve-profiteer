using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace eZet.EveProfiteer.Repository {
    public class DbContextRepository<T> : IRepository<T> where T : class {

        private DbContext dbContext { get; set; }

        private DbSet<T> Set {
            get { return dbContext.Set<T>(); }
        }

        public DbContextRepository(DbContext dbContext) {
            this.dbContext = dbContext;
        }

        public T Create() {
            return Set.Create();
        }

        public bool Validate { get; set; }

        public IQueryable<T> GetAll() {
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
            return dbContext.SaveChanges();
        }

        public T Find(int pkey) {
            return Set.Find(pkey);
        }

        public IQueryable<T> Find(Expression<Func<T, bool>> filter) {
            return Set.Where(filter);
        }

        public void Dispose() {
            dbContext.Dispose();
        }
    }
}
