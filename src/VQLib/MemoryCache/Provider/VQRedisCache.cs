using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using VQLib.Util;

namespace VQLib.MemoryCache.Provider
{
    public class VQRedisCache : IVQCache
    {
        private readonly IConnectionMultiplexer _connection;

        public VQRedisCache(IConnectionMultiplexer connection)
        {
            _connection = connection;
        }

        public Task<T> Get<T>(string key, T defaultValue = default)
        {
            return GetPrivate(key, defaultValue);
        }

        public Task<T> Set<T>(string key, T @object, TimeSpan duration = default)
        {
            return SetPrivate<T>(key, @object, duration);
        }

        public async Task<T> GetOrSet<T>(string key, Func<T> getObjectFunc, TimeSpan duration = default)
        {
            var db = _connection.GetDatabase();

            var value = await GetPrivate<T>(key, default, db);
            if (value.Equals(default(T)))
                return await SetPrivate(key, getObjectFunc(), duration, db);
            return value;
        }

        public Task Remove(string key)
        {
            return RemovePrivate(key);
        }

        private async Task<T> SetPrivate<T>(string key, T @object, TimeSpan duration = default, IDatabase db = null)
        {
            db ??= _connection.GetDatabase();

            if (duration == default)
                duration = TimeSpan.FromHours(2);

            var stringValue = @object.ToJson();
            await db.StringSetAsync(key, stringValue, duration);
            return @object;
        }

        private async Task<T> GetPrivate<T>(string key, T defaultValue, IDatabase db = null)
        {
            db ??= _connection.GetDatabase();

            var valueString = await db.StringGetAsync(key);
            if (!valueString.IsNullOrEmpty)
                return ((string)valueString).FromJson<T>();
            return defaultValue;
        }

        public async Task RemovePrivate(string key, IDatabase db = null)
        {
            db ??= _connection.GetDatabase();

            await db.KeyDeleteAsync(key);
        }
    }
}