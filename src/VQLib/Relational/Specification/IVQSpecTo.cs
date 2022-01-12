using System.Linq;
using VQLib.Relational.Entity;

namespace VQLib.Relational.Specification
{
    public interface IVQSpecTo<TSrc, TDest> where TSrc : VQBaseEntity
    {
        IQueryable<TDest> Specify(IQueryable<TSrc> query);

        IOrderedQueryable<TDest> Order(IQueryable<TDest> query);
    }
}