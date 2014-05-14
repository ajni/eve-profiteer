using System;
using System.Collections.Generic;
using System.Linq;

namespace eZet.EveProfiteer.Repository {
    public interface IRepository<T> : IDisposable where T : class {
        bool Validate { get; set; }
        T Create();

        IQueryable<T> All();

        T Add(T entity);

        IEnumerable<T> AddRange(IEnumerable<T> entities);

        T Remove(T entity);

        IEnumerable<T> RemoveRange(IEnumerable<T> entities);

        int SaveChanges();

        T Find(int pkey);
    }
}