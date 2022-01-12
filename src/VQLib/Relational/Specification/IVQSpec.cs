using System.Linq;
using VQLib.Relational.Entity;

namespace VQLib.Relational.Specification
{
    public interface IVQSpec<T> where T : VQBaseEntity
    {
        IQueryable<T> Specify(IQueryable<T> query);

        IOrderedQueryable<T> Order(IQueryable<T> query);
    }
}