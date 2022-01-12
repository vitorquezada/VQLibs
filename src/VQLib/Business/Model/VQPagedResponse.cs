using System.Collections.Generic;
using Newtonsoft.Json;
using X.PagedList;

namespace VQLib.Business.Model
{
    [JsonObject]
    public class VQPagedResponse<T> : StaticPagedList<T>
    {
        public VQPagedResponse(IEnumerable<T> subset, IPagedList metaData) : base(subset, metaData)
        {
        }

        public VQPagedResponse(IEnumerable<T> subset, int pageNumber, int pageSize, int totalItemCount) : base(subset, pageNumber, pageSize, totalItemCount)
        {
        }

        public List<T> Data { get => Subset; }
    }
}