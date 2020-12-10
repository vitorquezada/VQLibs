using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VQLibs.Relational.Entity;
using VQLibs.Relational.Repository.Interface;
using VQLibs.Relational.Service.Interface;
using VQLibs.Relational.Specification;
using X.PagedList;

namespace VQLibs.Relational.Service.Implementation
{
    public class VQBaseService<T> : IVQBaseService<T> where T : VQBaseEntity
    {
        private readonly IVQBaseRepository<T> _repository;

        public VQBaseService(IVQBaseRepository<T> repository)
        {
            _repository = repository;
        }

        public virtual async Task<bool> Any(IVQSpec<T> spec, CancellationToken cancellationToken = default)
        {
            return await _repository.Any(spec, cancellationToken);
        }

        public virtual async Task<bool> Any(ulong id, CancellationToken cancellationToken = default)
        {
            return await _repository.Any(id, cancellationToken);
        }

        public virtual async Task<bool> Any(Guid key, CancellationToken cancellationToken = default)
        {
            return await _repository.Any(key, cancellationToken);
        }

        public virtual async Task<bool> Any(CancellationToken cancellationToken = default)
        {
            return await _repository.Any(cancellationToken);
        }

        public virtual async Task<long> Count(CancellationToken cancellationToken = default)
        {
            return await _repository.Count(cancellationToken);
        }

        public virtual async Task<long> Count(IVQSpec<T> spec, CancellationToken cancellationToken = default)
        {
            return await _repository.Count(spec, cancellationToken);
        }

        public virtual async Task Delete(ulong id, CancellationToken cancellationToken = default)
        {
            await _repository.Delete(id, cancellationToken);
        }

        public virtual async Task Delete(IEnumerable<ulong> ids, CancellationToken cancellationToken = default)
        {
            await _repository.Delete(ids, cancellationToken);
        }

        public virtual async Task<T> First(IVQSpec<T> spec, CancellationToken cancellationToken = default)
        {
            return await _repository.First(spec, cancellationToken);
        }

        public virtual async Task<TDest> First<TDest>(IVQSpecTo<T, TDest> spec, CancellationToken cancellationToken = default)
        {
            return await _repository.First(spec, cancellationToken);
        }

        public virtual async Task<T> Get(ulong id, CancellationToken cancellationToken = default)
        {
            return await _repository.Get(id, cancellationToken);
        }

        public virtual async Task<T> Get(Guid key, CancellationToken cancellationToken = default)
        {
            return await _repository.Get(key, cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> Get(CancellationToken cancellationToken = default)
        {
            return await _repository.Get(cancellationToken);
        }

        public virtual async Task<IPagedList<T>> Get(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            return await _repository.Get(pageNumber, pageSize, cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> Get(IVQSpec<T> spec, CancellationToken cancellationToken = default)
        {
            return await _repository.Get(spec, cancellationToken);
        }

        public virtual async Task<IPagedList<T>> Get(IVQSpec<T> spec, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            return await _repository.Get(spec, pageNumber, pageSize, cancellationToken);
        }

        public virtual async Task<IEnumerable<TDest>> Get<TDest>(IVQSpecTo<T, TDest> spec, CancellationToken cancellationToken = default)
        {
            return await _repository.Get(spec, cancellationToken);
        }

        public virtual async Task<IPagedList<TDest>> Get<TDest>(IVQSpecTo<T, TDest> spec, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            return await _repository.Get(spec, pageNumber, pageSize, cancellationToken);
        }

        public virtual async Task<T> InsertUpdate(T entity, CancellationToken cancellationToken = default)
        {
            return await _repository.InsertUpdate(entity, cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> InsertUpdate(IList<T> entities, CancellationToken cancellationToken = default)
        {
            return await _repository.InsertUpdate(entities, cancellationToken);
        }
    }
}
