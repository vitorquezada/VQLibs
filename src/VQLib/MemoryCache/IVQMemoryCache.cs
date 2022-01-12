using System;
using System.Threading.Tasks;

namespace VQLib.MemoryCache
{
    public interface IVQMemoryCache
    {
        public Task<T> GetOrSet<T>(string key, Func<T> getObjectFunc, TimeSpan duration = default);

        public Task<T> Remove<T>(string key);
    }
}