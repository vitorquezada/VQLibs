using System.Linq;
using VQLibs.Relational.Entity;

namespace VQLibs.Relational.Specification
{
    public interface IVQSpec<T> where T : VQBaseEntity
    {
        IQueryable<T> Specify(IQueryable<T> query);

        IOrderedQueryable<T> Order(IQueryable<T> query);
    }
}
