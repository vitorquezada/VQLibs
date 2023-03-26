using Microsoft.EntityFrameworkCore;
using VQLib.Relational.Entity;
using VQLib.Relational.Specification;
using X.PagedList;

namespace VQLib.Relational.Repository
{
    public interface IVQRepository<T> : IVQRepositoryWithoutTenant<T> where T : VQBaseEntityTenant
    {
        Task<bool> AnyUnsafe(IVQSpec<T> spec, CancellationToken cancellationToken = default);

        Task<bool> AnyUnsafe(long id, CancellationToken cancellationToken = default);

        Task<bool> AnyUnsafe(string key, CancellationToken cancellationToken = default);

        Task<bool> AnyUnsafe(CancellationToken cancellationToken = default);

        Task<long> CountUnsafe(CancellationToken cancellationToken = default);

        Task<long> CountUnsafe(IVQSpec<T> spec, CancellationToken cancellationToken = default);

        Task<T> FirstUnsafe(IVQSpec<T> spec, CancellationToken cancellationToken = default);

        Task<TDest> FirstUnsafe<TDest>(IVQSpecTo<T, TDest> spec, CancellationToken cancellationToken = default);

        Task<T> GetUnsafe(long id, CancellationToken cancellationToken = default);

        Task<T> GetUnsafe(string key, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> GetUnsafe(CancellationToken cancellationToken = default);

        Task<IPagedList<T>> GetUnsafe(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> GetUnsafe(IVQSpec<T> spec, CancellationToken cancellationToken = default);

        Task<IPagedList<T>> GetUnsafe(IVQSpec<T> spec, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

        Task<IEnumerable<TDest>> GetUnsafe<TDest>(IVQSpecTo<T, TDest> spec, CancellationToken cancellationToken = default);

        Task<IPagedList<TDest>> GetUnsafe<TDest>(IVQSpecTo<T, TDest> spec, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    }

    public interface IVQRepositoryWithoutTenant<T> : IDisposable, IAsyncDisposable where T : VQBaseEntity
    {
        Task<bool> Any(IVQSpec<T> spec, CancellationToken cancellationToken = default);

        Task<bool> Any(long id, CancellationToken cancellationToken = default);

        Task<bool> Any(string key, CancellationToken cancellationToken = default);

        Task<bool> Any(CancellationToken cancellationToken = default);

        Task<long> Count(CancellationToken cancellationToken = default);

        Task<long> Count(IVQSpec<T> spec, CancellationToken cancellationToken = default);

        Task<int> Delete(long id, CancellationToken cancellationToken = default);

        Task<int> Delete(long id, bool saveChanges, CancellationToken cancellationToken = default);

        Task<int> Delete(IEnumerable<long> ids, CancellationToken cancellationToken = default);

        Task<int> Delete(IEnumerable<long> ids, bool saveChanges, CancellationToken cancellationToken = default);

        Task<T> First(IVQSpec<T> spec, CancellationToken cancellationToken = default);

        Task<TDest> First<TDest>(IVQSpecTo<T, TDest> spec, CancellationToken cancellationToken = default);

        Task<T> Get(long id, CancellationToken cancellationToken = default);

        Task<T> Get(string key, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> Get(CancellationToken cancellationToken = default);

        Task<IPagedList<T>> Get(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> Get(IVQSpec<T> spec, CancellationToken cancellationToken = default);

        Task<IPagedList<T>> Get(IVQSpec<T> spec, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

        Task<IEnumerable<TDest>> Get<TDest>(IVQSpecTo<T, TDest> spec, CancellationToken cancellationToken = default);

        Task<IPagedList<TDest>> Get<TDest>(IVQSpecTo<T, TDest> spec, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

        IQueryable<T> GetCollection();

        IQueryable<T> GetCollectionUnsafe();

        DbSet<T> GetDbSet();

        Task<long> GetIdByKey(string key, CancellationToken cancellationToken = default);

        Task<T> InsertUpdate(T entity, CancellationToken cancellationToken = default);

        Task<T> InsertUpdate(T entity, bool saveChanges, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> InsertUpdate(IList<T> entities, CancellationToken cancellationToken = default);

        Task<int> SaveChanges(CancellationToken cancellationToken = default);
    }
}