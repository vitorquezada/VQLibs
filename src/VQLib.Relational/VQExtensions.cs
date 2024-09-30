using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace VQLib.Relational
{
    public static class VQExtensions
    {
        public static async Task<IPagedList<T>> ToPagedListAsync<T>(this IQueryable<T> superset, int pageNumber, int pageSize, int? totalSetCount, CancellationToken ct)
        {
            if (superset == null)
            {
                throw new ArgumentNullException(nameof(superset));
            }

            if (pageNumber < 1)
            {
                throw new ArgumentOutOfRangeException($"pageNumber = {pageNumber}. PageNumber cannot be below 1.");
            }

            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException($"pageSize = {pageSize}. PageSize cannot be less than 1.");
            }

            int totalCount = totalSetCount ?? await superset.CountAsync(ct);

            List<T> subset;

            if (totalCount > 0)
            {
                var skip = (pageNumber - 1) * pageSize;

                subset = await superset.Skip(skip).Take(pageSize).ToListAsync(ct);
            }
            else
            {
                subset = new List<T>();
            }

            return new StaticPagedList<T>(subset, pageNumber, pageSize, totalCount);
        }
    }
}