using System.Collections.Generic;
using System.Linq;
using X.PagedList;

namespace VQLib.Business.Model
{
    public class VQPagedResponse<T>
    {
        private readonly StaticPagedList<T> _pageList;

        public VQPagedResponse(IPagedList<T> pageList)
        {
            _pageList = new StaticPagedList<T>(pageList.ToList(), pageList);
        }

        public VQPagedResponse(IEnumerable<T> subset, IPagedList metaData)
        {
            _pageList = new StaticPagedList<T>(subset, metaData);
        }

        public VQPagedResponse(IEnumerable<T> subset, int pageNumber, int pageSize, int totalItemCount)
        {
            _pageList = new StaticPagedList<T>(subset, pageNumber, pageSize, totalItemCount);
        }

        public List<T> Data { get => _pageList.ToList(); }

        public int PageCount { get => _pageList.PageCount; }

        public int TotalItemCount { get => _pageList.TotalItemCount; }

        public int PageNumber { get => _pageList.PageNumber; }

        public int PageSize { get => _pageList.PageSize; }

        public bool HasPreviousPage { get => _pageList.HasPreviousPage; }

        public bool HasNextPage { get => _pageList.HasNextPage; }

        public bool IsFirstPage { get => _pageList.IsFirstPage; }

        public bool IsLastPage { get => _pageList.IsLastPage; }

        public int FirstItemOnPage { get => _pageList.FirstItemOnPage; }

        public int LastItemOnPage { get => _pageList.LastItemOnPage; }
    }
}