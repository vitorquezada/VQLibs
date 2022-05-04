namespace VQLib.Util
{
    public static class VQCollectionExpression
    {
        public static bool ListHasItem<T>(this IEnumerable<T> list, Func<T, bool>? filter = null)
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

        public static List<List<T>> ChunkBySize<T>(this IEnumerable<T> list, Func<T, long> metricBytes, long maxChunkSize)
        {
            if (list == null || !list.Any())
                return new List<List<T>>();

            return list
                .Aggregate(new
                {
                    Sum = 0L,
                    Current = (List<T>?)null,
                    Result = new List<List<T>>()
                },
                (agg, item) =>
                {
                    long value = metricBytes(item);
                    if (agg.Current == null || agg.Sum + value > maxChunkSize)
                    {
                        var current = new List<T> { item };
                        agg.Result.Add(current);
                        return new { Sum = value, Current = (List<T>?)current, agg.Result };
                    }

                    agg.Current.Add(item);
                    return new { Sum = agg.Sum + value, Current = (List<T>?)agg.Current, agg.Result };
                }).Result;
        }
    }
}