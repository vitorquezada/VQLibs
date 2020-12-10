using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VQLibs.Relational.Entity;
using VQLibs.Relational.Specification;
using X.PagedList;

namespace VQLibs.Relational.Repository.Interface
{
    public interface IVQBaseRepository<T> where T : VQBaseEntity
    {
        Task<T> Get(ulong id, CancellationToken cancellationToken = default);
        Task<T> Get(Guid key, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> Get(CancellationToken cancellationToken = default);
        Task<IPagedList<T>> Get(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> Get(IVQSpec<T> spec, CancellationToken cancellationToken = default);
        Task<IPagedList<T>> Get(IVQSpec<T> spec, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

        Task<IEnumerable<TDest>> Get<TDest>(IVQSpecTo<T, TDest> spec, CancellationToken cancellationToken = default);
        Task<IPagedList<TDest>> Get<TDest>(IVQSpecTo<T, TDest> spec, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

        Task<T> First(IVQSpec<T> spec, CancellationToken cancellationToken = default);
        Task<TDest> First<TDest>(IVQSpecTo<T, TDest> spec, CancellationToken cancellationToken = default);

        Task<long> Count(CancellationToken cancellationToken = default);
        Task<long> Count(IVQSpec<T> spec, CancellationToken cancellationToken = default);

        Task<bool> Any(IVQSpec<T> spec, CancellationToken cancellationToken = default);
        Task<bool> Any(ulong id, CancellationToken cancellationToken = default);
        Task<bool> Any(Guid key, CancellationToken cancellationToken = default);
        Task<bool> Any(CancellationToken cancellationToken = default);

        Task<T> InsertUpdate(T entity, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> InsertUpdate(IList<T> entities, CancellationToken cancellationToken = default);

        Task Delete(ulong id, CancellationToken cancellationToken = default);
        Task Delete(IEnumerable<ulong> ids, CancellationToken cancellationToken = default);

        Task<int> SaveChanges();
    }
}
