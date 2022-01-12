using System.Linq;
using VQLib.Relational.Entity;

namespace VQLib.Relational.Specification
{
    public class VQGetIdByKey<T> : IVQSpecTo<T, long> where T : VQBaseEntity
    {
        private readonly string _key;

        public VQGetIdByKey(string key)
        {
            _key = key;
        }

        public IOrderedQueryable<long> Order(IQueryable<long> query)
        {
            return query.OrderBy(x => x);
        }

        public IQueryable<long> Specify(IQueryable<T> query)
        {
            return query.Where(x => x.Key == _key).Select(x => x.Id);
        }
    }
}