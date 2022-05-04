namespace VQLib.MemoryCache
{
    public interface IVQCache
    {
        Task<T?> Get<T>(string key, T? defaultValue = default);

        Task Set<T>(string key, T @object, TimeSpan duration = default);

        Task<T?> GetOrSet<T>(string key, Func<Task<T>> getObjectFunc, TimeSpan duration = default);

        Task Remove(string key);
    }
}