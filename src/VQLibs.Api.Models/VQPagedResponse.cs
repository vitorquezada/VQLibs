using Newtonsoft.Json;
using System.Collections.Generic;
using X.PagedList;

namespace VQLibs.Api.Models
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
