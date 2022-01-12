using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VQLib.Relational.Entity;
using VQLib.Relational.Specification;
using VQLib.Session;
using X.PagedList;

namespace VQLib.Relational.Repository
{
    public class VQRepository<T> : VQRepositoryWithoutTenant<T>, IVQRepository<T> where T : VQBaseEntityTenant, new()
    {
        private readonly IVQSessionService _sessionService;

        protected override IQueryable<T> _dbSet => base._dbSet.Where(x => x.TenantId == _sessionService.TenantId);
        protected IQueryable<T> _dbSetUnsafe => base._dbSet;

        public VQRepository(DbContext dbContext, IVQSessionService sessionService) : base(dbContext)
        {
            _sessionService = sessionService;
        }

        #region Any

        public virtual Task<bool> AnyUnsafe(IVQSpec<T> spec, CancellationToken cancellationToken = default) => SetSpecificationUnsafe(spec).AnyAsync(cancellationToken);

        public virtual Task<bool> AnyUnsafe(long id, CancellationToken cancellationToken = default) => _dbSetUnsafe.AnyAsync(x => x.Id == id, cancellationToken);

        public virtual Task<bool> AnyUnsafe(string key, CancellationToken cancellationToken = default) => _dbSetUnsafe.AnyAsync(x => x.Key == key, cancellationToken);

        public virtual Task<bool> AnyUnsafe(CancellationToken cancellationToken = default) => _dbSetUnsafe.AnyAsync(cancellationToken);

        #endregion Any

        #region Count

        public virtual Task<long> CountUnsafe(CancellationToken cancellationToken = default) => _dbSetUnsafe.LongCountAsync(cancellationToken);

        public virtual Task<long> CountUnsafe(IVQSpec<T> spec, CancellationToken cancellationToken = default) => SetSpecificationUnsafe(spec).LongCountAsync(cancellationToken);

        #endregion Count

        #region First

        public virtual Task<T> FirstUnsafe(IVQSpec<T> spec, CancellationToken cancellationToken = default) => SetSpecificationUnsafe(spec).FirstOrDefaultAsync(cancellationToken);

        public virtual Task<TDest> FirstUnsafe<TDest>(IVQSpecTo<T, TDest> spec, CancellationToken cancellationToken = default) => SetSpecificationUnsafe(spec).FirstOrDefaultAsync(cancellationToken);

        #endregion First

        #region Get

        public virtual Task<T> GetUnsafe(long id, CancellationToken cancellationToken = default) => _dbSetUnsafe.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        public virtual Task<T> GetUnsafe(string key, CancellationToken cancellationToken = default) => _dbSetUnsafe.FirstOrDefaultAsync(x => x.Key == key, cancellationToken);

        public virtual async Task<IEnumerable<T>> GetUnsafe(CancellationToken cancellationToken = default) => await _dbSetUnsafe.ToListAsync(cancellationToken);

        public virtual Task<IPagedList<T>> GetUnsafe(int pageNumber, int pageSize, CancellationToken cancellationToken = default) => _dbSetUnsafe.ToPagedListAsync(pageNumber, pageSize, cancellationToken);

        public virtual async Task<IEnumerable<T>> GetUnsafe(IVQSpec<T> spec, CancellationToken cancellationToken = default) => await SetSpecificationUnsafe(spec).ToListAsync(cancellationToken);

        public virtual Task<IPagedList<T>> GetUnsafe(IVQSpec<T> spec, int pageNumber, int pageSize, CancellationToken cancellationToken = default) => SetSpecificationUnsafe(spec).ToPagedListAsync(pageNumber, pageSize, cancellationToken);

        public virtual async Task<IEnumerable<TDest>> GetUnsafe<TDest>(IVQSpecTo<T, TDest> spec, CancellationToken cancellationToken = default) => await SetSpecificationUnsafe(spec).ToListAsync(cancellationToken);

        public virtual Task<IPagedList<TDest>> GetUnsafe<TDest>(IVQSpecTo<T, TDest> spec, int pageNumber, int pageSize, CancellationToken cancellationToken = default) => SetSpecificationUnsafe(spec).ToPagedListAsync(pageNumber, pageSize, cancellationToken);

        #endregion Get

        private IQueryable<T> SetSpecificationUnsafe(IVQSpec<T> spec) => spec.Order(spec.Specify(_dbSetUnsafe));

        private IQueryable<TDest> SetSpecificationUnsafe<TDest>(IVQSpecTo<T, TDest> spec) => spec.Order(spec.Specify(_dbSetUnsafe));
    }

    public class VQRepositoryWithoutTenant<T> : IVQRepositoryWithoutTenant<T>, IDisposable, IAsyncDisposable where T : VQBaseEntity, new()
    {
        private readonly DbContext _dbContext;
        protected virtual IQueryable<T> _dbSet => _dbContext.Set<T>();

        public VQRepositoryWithoutTenant(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region Any

        public virtual Task<bool> Any(IVQSpec<T> spec, CancellationToken cancellationToken = default) => SetSpecification(spec).AnyAsync(cancellationToken);

        public virtual Task<bool> Any(long id, CancellationToken cancellationToken = default) => _dbSet.AnyAsync(x => x.Id == id, cancellationToken);

        public virtual Task<bool> Any(string key, CancellationToken cancellationToken = default) => _dbSet.AnyAsync(x => x.Key == key, cancellationToken);

        public virtual Task<bool> Any(CancellationToken cancellationToken = default) => _dbSet.AnyAsync(cancellationToken);

        #endregion Any

        #region Count

        public virtual Task<long> Count(CancellationToken cancellationToken = default) => _dbSet.LongCountAsync(cancellationToken);

        public virtual Task<long> Count(IVQSpec<T> spec, CancellationToken cancellationToken = default) => SetSpecification(spec).LongCountAsync(cancellationToken);

        #endregion Count

        #region First

        public virtual Task<T> First(IVQSpec<T> spec, CancellationToken cancellationToken = default) => SetSpecification(spec).FirstOrDefaultAsync(cancellationToken);

        public virtual Task<TDest> First<TDest>(IVQSpecTo<T, TDest> spec, CancellationToken cancellationToken = default) => SetSpecification(spec).FirstOrDefaultAsync(cancellationToken);

        #endregion First

        #region Get

        public virtual Task<T> Get(long id, CancellationToken cancellationToken = default) => _dbSet.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        public virtual Task<T> Get(string key, CancellationToken cancellationToken = default) => _dbSet.FirstOrDefaultAsync(x => x.Key == key, cancellationToken);

        public virtual async Task<IEnumerable<T>> Get(CancellationToken cancellationToken = default) => await _dbSet.ToListAsync(cancellationToken);

        public virtual Task<IPagedList<T>> Get(int pageNumber, int pageSize, CancellationToken cancellationToken = default) => _dbSet.ToPagedListAsync(pageNumber, pageSize, cancellationToken);

        public virtual async Task<IEnumerable<T>> Get(IVQSpec<T> spec, CancellationToken cancellationToken = default) => await SetSpecification(spec).ToListAsync(cancellationToken);

        public virtual Task<IPagedList<T>> Get(IVQSpec<T> spec, int pageNumber, int pageSize, CancellationToken cancellationToken = default) => SetSpecification(spec).ToPagedListAsync(pageNumber, pageSize, cancellationToken);

        public virtual async Task<IEnumerable<TDest>> Get<TDest>(IVQSpecTo<T, TDest> spec, CancellationToken cancellationToken = default) => await SetSpecification(spec).ToListAsync(cancellationToken);

        public virtual Task<IPagedList<TDest>> Get<TDest>(IVQSpecTo<T, TDest> spec, int pageNumber, int pageSize, CancellationToken cancellationToken = default) => SetSpecification(spec).ToPagedListAsync(pageNumber, pageSize, cancellationToken);

        #endregion Get

        #region InsertUpdate

        public virtual Task<T> InsertUpdate(T entity, CancellationToken cancellationToken = default) => InternalInsertUpdate(entity, true, cancellationToken);

        public virtual async Task<IEnumerable<T>> InsertUpdate(IList<T> entities, CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                entities[i] = await InternalInsertUpdate(entities[i], false, cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return entities;
        }

        #endregion InsertUpdate

        #region Delete

        public virtual async Task<int> Delete(long id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<T>().FindAsync(new object[] { id }, cancellationToken);
            _dbContext.Set<T>().Remove(entity);
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task<int> Delete(IEnumerable<long> ids, CancellationToken cancellationToken = default)
        {
            var entities = await Task.WhenAll(ids.Select(async x => await _dbContext.Set<T>().FindAsync(new object[] { x }, cancellationToken)));
            _dbContext.Set<T>().RemoveRange(entities);
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        #endregion Delete

        #region SaveChanges

        public Task<int> SaveChanges(CancellationToken cancellationToken = default) => _dbContext.SaveChangesAsync(cancellationToken);

        #endregion SaveChanges

        #region Dispose

        public void Dispose() => _dbContext.Dispose();

        public ValueTask DisposeAsync() => _dbContext.DisposeAsync();

        #endregion Dispose

        #region " PRIVATE METHODS "

        private IQueryable<T> SetSpecification(IVQSpec<T> spec) => spec.Order(spec.Specify(_dbSet));

        private IQueryable<TDest> SetSpecification<TDest>(IVQSpecTo<T, TDest> spec) => spec.Order(spec.Specify(_dbSet));

        private async Task<T> InternalInsertUpdate(T entity, bool saveChanges, CancellationToken cancellationToken = default)
        {
            var exist = entity.Id > 0
                ? await _dbSet.IgnoreQueryFilters().AnyAsync(x => x.Id == entity.Id, cancellationToken)
                : false;

            if (exist)
            {
                _dbContext.Set<T>().Update(entity);
            }
            else
            {
                await _dbContext.Set<T>().AddAsync(entity, cancellationToken);
            }

            if (saveChanges)
                await _dbContext.SaveChangesAsync(cancellationToken);

            return entity;
        }

        #endregion " PRIVATE METHODS "
    }
}