using System;
using System.Collections.Generic;
using System.Linq;

namespace VQLibs.Util.Extension
{
    public static class VQCollectionExtension
    {
        public static bool ListHasItem<T>(this IEnumerable<T> collection, Func<T, bool> filter = null)
        {
            return collection != null && (filter == null ? collection.Any() : collection.Any(filter));
        }
    }
}
