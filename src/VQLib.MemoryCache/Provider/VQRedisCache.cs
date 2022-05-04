using StackExchange.Redis;
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

        public Task<T?> Get<T>(string key, T? defaultValue = default)
        {
            return GetPrivate(key, defaultValue);
        }

        public async Task Set<T>(string key, T @object, TimeSpan duration = default)
        {
            await SetPrivate<T>(key, @object, duration);
        }

        public async Task<T?> GetOrSet<T>(string key, Func<Task<T>> getObjectFunc, TimeSpan duration = default)
        {
            var db = _connection.GetDatabase();

            var @default = default(T);
            var value = await GetPrivate(key, @default, db);
            if (!ReferenceEquals(value, @default))
                return value;

            value = await getObjectFunc();
            await SetPrivate(key, value, duration, db);
            return value;
        }

        public Task Remove(string key)
        {
            return RemovePrivate(key);
        }

        private async Task SetPrivate<T>(string key, T @object, TimeSpan duration = default, IDatabase? db = null)
        {
            db ??= _connection.GetDatabase();

            if (duration == default)
                duration = TimeSpan.FromHours(2);

            var stringValue = @object.ToJson();
            await db.StringSetAsync(key, stringValue, duration);
        }

        private async Task<T?> GetPrivate<T>(string key, T? defaultValue, IDatabase? db = null)
        {
            db ??= _connection.GetDatabase();

            var valueString = await db.StringGetAsync(key);
            if (!valueString.IsNullOrEmpty)
                return ((string)valueString).FromJson<T>();
            return defaultValue;
        }

        public async Task RemovePrivate(string key, IDatabase? db = null)
        {
            db ??= _connection.GetDatabase();
            await db.KeyDeleteAsync(key);
        }
    }
}