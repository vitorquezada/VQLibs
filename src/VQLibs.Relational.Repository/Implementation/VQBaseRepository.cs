using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VQLibs.Relational.Entity;
using VQLibs.Relational.Repository.Interface;
using VQLibs.Relational.Specification;
using X.PagedList;

namespace VQLibs.Relational.Repository.Implementation
{
    public class VQBaseRepository<T> : IVQBaseRepository<T> where T : VQBaseEntity, new()
    {
        private readonly DbContext _dbContext;
        private readonly DbSet<T> _dbSet;

        public VQBaseRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }

        #region Any

        public virtual Task<bool> Any(IVQSpec<T> spec, CancellationToken cancellationToken = default)
        {
            return SetSpecification(spec).AnyAsync(cancellationToken);
        }

        public virtual Task<bool> Any(ulong id, CancellationToken cancellationToken = default)
        {
            return _dbSet.AnyAsync(x => x.Id == id, cancellationToken);
        }

        public virtual Task<bool> Any(Guid key, CancellationToken cancellationToken = default)
        {
            return _dbSet.AnyAsync(x => x.Key == key, cancellationToken);
        }

        public virtual Task<bool> Any(CancellationToken cancellationToken = default)
        {
            return _dbSet.AnyAsync();
        }

        #endregion

        #region Count

        public virtual Task<long> Count(CancellationToken cancellationToken = default)
        {
            return _dbSet.LongCountAsync(cancellationToken);
        }

        public virtual Task<long> Count(IVQSpec<T> spec, CancellationToken cancellationToken = default)
        {
            return SetSpecification(spec).LongCountAsync(cancellationToken);
        }

        #endregion

        #region Delete

        public virtual async Task Delete(ulong id, CancellationToken cancellationToken = default)
        {
            _dbSet.Remove(new T { Id = id });
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual Task Delete(IEnumerable<ulong> ids, CancellationToken cancellationToken = default)
        {
            _dbSet.RemoveRange(ids.Select(x => new T { Id = x }));
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        #endregion

        #region First

        public virtual Task<T> First(IVQSpec<T> spec, CancellationToken cancellationToken = default)
        {
            return SetSpecification(spec).FirstOrDefaultAsync(cancellationToken);
        }

        public virtual Task<TDest> First<TDest>(IVQSpecTo<T, TDest> spec, CancellationToken cancellationToken = default)
        {
            return SetSpecification(spec).FirstOrDefaultAsync(cancellationToken);
        }

        #endregion

        #region Get

        public virtual Task<T> Get(ulong id, CancellationToken cancellationToken = default)
        {
            return _dbSet.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public virtual Task<T> Get(Guid key, CancellationToken cancellationToken = default)
        {
            return _dbSet.FirstOrDefaultAsync(x => x.Key == key, cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> Get(CancellationToken cancellationToken = default)
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }

        public virtual Task<IPagedList<T>> Get(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            return _dbSet.AsQueryable().ToPagedListAsync(pageNumber, pageSize, cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> Get(IVQSpec<T> spec, CancellationToken cancellationToken = default)
        {
            return await SetSpecification(spec).ToListAsync(cancellationToken);
        }

        public virtual Task<IPagedList<T>> Get(IVQSpec<T> spec, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            return SetSpecification(spec).ToPagedListAsync(pageNumber, pageSize, cancellationToken);
        }

        public virtual async Task<IEnumerable<TDest>> Get<TDest>(IVQSpecTo<T, TDest> spec, CancellationToken cancellationToken = default)
        {
            return await SetSpecification(spec).ToListAsync(cancellationToken);
        }

        public virtual Task<IPagedList<TDest>> Get<TDest>(IVQSpecTo<T, TDest> spec, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            return SetSpecification(spec).ToPagedListAsync(pageNumber, pageSize, cancellationToken);
        }

        #endregion

        #region InsertUpdate

        public virtual Task<T> InsertUpdate(T entity, CancellationToken cancellationToken = default)
        {
            return InternalInsertUpdate(entity, true, cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> InsertUpdate(IList<T> entities, CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                entities[i] = await InternalInsertUpdate(entities[i], false, cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return entities;
        }

        #endregion

        #region SaveChanges

        public async Task<int> SaveChanges()
        {
            return await _dbContext.SaveChangesAsync();
        }

        #endregion

        #region " PRIVATE METHODS "

        private IQueryable<T> SetSpecification(IVQSpec<T> spec)
        {
            return spec.Order(spec.Specify(_dbSet));
        }

        private IQueryable<TDest> SetSpecification<TDest>(IVQSpecTo<T, TDest> spec)
        {
            return spec.Order(spec.Specify(_dbSet));
        }

        private async Task<T> InternalInsertUpdate(T entity, bool saveChanges, CancellationToken cancellationToken = default)
        {
            var exist = await Any(entity.Id, cancellationToken);
            if (exist)
            {
                _dbSet.Update(entity);
            }
            else
            {
                await _dbSet.AddAsync(entity, cancellationToken);
            }

            if (saveChanges)
                await _dbContext.SaveChangesAsync(cancellationToken);

            return entity;
        }

        #endregion " PRIVATE METHODS "
    }
}
