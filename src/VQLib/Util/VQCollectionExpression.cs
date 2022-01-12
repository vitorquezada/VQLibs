using System;
using System.Collections.Generic;
using System.Linq;

namespace VQLib.Util
{
    public static class VQCollectionExpression
    {
        public static bool ListHasItem<T>(this IEnumerable<T> list, Func<T, bool> filter = null)
        {
            return list != null && (filter == null ? list.Any() : list.Any(filter));
        }

        public static string Join<T>(this IEnumerable<T> list, string separator) => list.ListHasItem() ? string.Join(separator, list) : string.Empty;

        public static void AddRange<T>(this SortedSet<T> set, IEnumerable<T> items)
        {
            if (items == null || !items.Any())
                return;

            set ??= new SortedSet<T>();
            foreach (var item in items)
                set.Add(item);
        }
    }
}