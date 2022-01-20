using System;
using System.Threading.Tasks;

namespace VQLib.MemoryCache
{
    public interface IVQCache
    {
        Task<T> Get<T>(string key, T defaultValue = default);

        Task<T> Set<T>(string key, T @object, TimeSpan duration = default);

        Task<T> GetOrSet<T>(string key, Func<T> getObjectFunc, TimeSpan duration = default);

        Task Remove(string key);
    }
}