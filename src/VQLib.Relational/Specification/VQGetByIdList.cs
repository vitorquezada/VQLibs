using System;
using System.Collections.Generic;
using System.Linq;
using VQLib.Relational.Entity;

namespace VQLib.Relational.Specification
{
    public class VQGetByIdList<T> : IVQSpec<T> where T : VQBaseEntity
    {
        private readonly IEnumerable<long> _ids;

        public VQGetByIdList(IEnumerable<long> ids)
        {
            _ids = ids ?? Array.Empty<long>();
        }

        public IQueryable<T> Specify(IQueryable<T> query)
        {
            return query.Where(x => _ids.Contains(x.Id));
        }
    }
}