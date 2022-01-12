using System;
using System.Collections.Generic;
using System.Linq;
using VQLib.Relational.Entity;
using VQLib.Util;

namespace VQLib.Relational.Specification
{
    public abstract class VQGetByKeyList<T> : IVQSpec<T> where T : VQBaseEntity
    {
        private readonly IEnumerable<string> _key;
        private readonly bool _keyIsCaseSensitive;

        public VQGetByKeyList(IEnumerable<string> key, bool keyIsCaseSensitive = false)
        {
            _key = (key ?? Array.Empty<string>()).Where(x => x.IsNotNullOrWhiteSpace());
            _keyIsCaseSensitive = keyIsCaseSensitive;
        }

        public virtual IOrderedQueryable<T> Order(IQueryable<T> query)
        {
            return query.OrderBy(x => x.Id);
        }

        public virtual IQueryable<T> Specify(IQueryable<T> query)
        {
            return _keyIsCaseSensitive
                ? query.Where(x => _key.Contains(x.Key))
                : query.Where(x => _key.Select(y => y.ToLower()).Contains(x.Key.ToLower()));
        }
    }
}