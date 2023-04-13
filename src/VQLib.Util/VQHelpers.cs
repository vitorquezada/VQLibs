using System.Reflection;

namespace VQLib.Util
{
    public static class VQHelpers
    {
        private static Dictionary<string, Attribute> _cacheCustomAttribute = new Dictionary<string, Attribute>();

        public static TAttribute GetCustomAttribute<T, TAttribute>(string propertyName) where TAttribute : Attribute
        {
            var type = typeof(T);
            if (type == null || string.IsNullOrWhiteSpace(type.FullName))
                throw new Exception("Error on get type");

            if (!_cacheCustomAttribute.ContainsKey($"{type.FullName}.{propertyName}"))
            {
                var attribute = type.GetMember(propertyName).FirstOrDefault()?.GetCustomAttribute<TAttribute>(true);
                if (attribute == null)
                    throw new NullReferenceException();
                return attribute;
            }

            return (TAttribute)_cacheCustomAttribute[propertyName];
        }

        public static TAttribute GetCustomAttribute<T, TAttribute>() where TAttribute : Attribute
        {
            var type = typeof(T);
            if (type == null || string.IsNullOrWhiteSpace(type.FullName))
                throw new Exception("Error on get type");

            if (!_cacheCustomAttribute.ContainsKey(type.FullName))
            {
                var attribute = typeof(T).GetCustomAttribute<TAttribute>(true);
                if (attribute == null)
                    throw new NullReferenceException();
                return attribute;
            }

            return (TAttribute)_cacheCustomAttribute[type.FullName];
        }
    }
}