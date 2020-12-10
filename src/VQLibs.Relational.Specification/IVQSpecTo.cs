using System.Linq;
using VQLibs.Relational.Entity;

namespace VQLibs.Relational.Specification
{
    public interface IVQSpecTo<TSrc, TDest> where TSrc : VQBaseEntity
    {
        IQueryable<TDest> Specify(IQueryable<TSrc> query);

        IOrderedQueryable<TDest> Order(IQueryable<TDest> query);
    }
}
