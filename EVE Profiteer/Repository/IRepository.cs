using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace eZet.EveProfiteer.Repository {
    public interface IRepository<T> : IDisposable where T : class {



        bool Validate { get; set; }
        T Create();

        IQueryable<T> GetAll();

        T Add(T entity);

        IEnumerable<T> AddRange(IEnumerable<T> entities);

        T Remove(T entity);

        IEnumerable<T> RemoveRange(IEnumerable<T> entities);

        int SaveChanges();

        T Find(int pkey);

        IQueryable<T> Find(Expression<Func<T, bool>> filter);

    }
}
