using System.Linq;
using VQLib.Relational.Entity;

namespace VQLib.Relational.Specification
{
    public class VQGetKeyById<T> : IVQSpecTo<T, string> where T : VQBaseEntity
    {
        private readonly long _id;

        public VQGetKeyById(long id)
        {
            _id = id;
        }

        public IOrderedQueryable<string> Order(IQueryable<string> query)
        {
            return query.OrderBy(x => x);
        }

        public IQueryable<string> Specify(IQueryable<T> query)
        {
            return query.Where(x => x.Id == _id).Select(x => x.Key);
        }
    }
}