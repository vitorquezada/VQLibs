using System.Collections.Generic;
using System.Linq;
using X.PagedList;

namespace VQLib.Business.Model
{
    public class VQPagedResponse<T> : IPagedList
    {
        public VQPagedResponse()
        {
            FillModel(new StaticPagedList<T>(new List<T>(), 1, 1, 0));
        }

        public VQPagedResponse(IPagedList<T> pageList)
        {
            FillModel(new StaticPagedList<T>(pageList.ToList(), pageList));
        }

        public VQPagedResponse(IEnumerable<T> subset, IPagedList metaData)
        {
            FillModel(new StaticPagedList<T>(subset, metaData));
        }

        public VQPagedResponse(IEnumerable<T> subset, int pageNumber, int pageSize, int totalItemCount)
        {
            FillModel(new StaticPagedList<T>(subset, pageNumber, pageSize, totalItemCount));
        }

        public List<T> Data { get; set; }

        public int FirstItemOnPage { get; set; }

        public bool HasNextPage { get; set; }

        public bool HasPreviousPage { get; set; }

        public bool IsFirstPage { get; set; }

        public bool IsLastPage { get; set; }

        public int LastItemOnPage { get; set; }

        public int PageCount { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalItemCount { get; set; }

        private void FillModel(StaticPagedList<T> pagedList)
        {
            Data = pagedList.ToList();
            FirstItemOnPage = pagedList.FirstItemOnPage;
            HasNextPage = pagedList.HasNextPage;
            HasPreviousPage = pagedList.HasPreviousPage;
            IsFirstPage = pagedList.IsFirstPage;
            IsLastPage = pagedList.IsLastPage;
            LastItemOnPage = pagedList.LastItemOnPage;
            PageCount = pagedList.PageCount;
            PageNumber = pagedList.PageNumber;
            PageSize = pagedList.PageSize;
            TotalItemCount = pagedList.TotalItemCount;
        }
    }
}