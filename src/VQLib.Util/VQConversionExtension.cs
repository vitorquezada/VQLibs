namespace VQLib.Util
{
    public static class VQConversionExtension
    {
        public static void CopyPropertiesTo<TSource, TDest>(this TSource source, TDest dest)
        {
            var sourceProperties = typeof(TSource).GetProperties().Where(x => x.CanRead).ToList();
            var destProperties = typeof(TDest).GetProperties().Where(x => x.CanWrite).ToList();

            foreach (var destProperty in destProperties)
            {
                var property = destProperty;
                var convertProperty = sourceProperties.FirstOrDefault(prop => prop.Name == property.Name);
                if (convertProperty != null)
                    convertProperty.SetValue(dest, Convert.ChangeType(destProperty.GetValue(source), convertProperty.PropertyType));
            }
        }

        public static byte[] StringHexToByte(this string @string, params string[] separators)
        {
            if (string.IsNullOrWhiteSpace(@string))
                return new byte[0];

            var stringWithoutSeparators = @string;
            foreach (var separator in separators)
            {
                stringWithoutSeparators = stringWithoutSeparators.Replace(separator, string.Empty);
            }

            return Enumerable
                .Range(0, stringWithoutSeparators.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(stringWithoutSeparators.Substring(x, 2), 16))
                .ToArray();
        }

        public static Guid StringToGuid(this string x) => Guid.TryParse(x, out Guid result)
            ? result
            : Guid.Empty;

        public static List<Guid> StringToGuidList(this string text, params string[] separator) => !string.IsNullOrWhiteSpace(text)
                                    ? new List<Guid>()
            : text.Split(separator, StringSplitOptions.RemoveEmptyEntries).Select(x => x.StringToGuid()).ToList();
    }
}