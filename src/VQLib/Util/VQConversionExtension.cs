using System;
using System.Collections.Generic;
using System.Linq;

namespace VQLib.Util
{
    public static class VQConversionExtension
    {
        public static List<Guid> StringToGuidList(this string text, params string[] separator) => !string.IsNullOrWhiteSpace(text)
            ? new List<Guid>()
            : text.Split(separator, StringSplitOptions.RemoveEmptyEntries).Select(x => x.StringToGuid()).ToList();

        public static Guid StringToGuid(this string x) => Guid.TryParse(x, out Guid result)
            ? result
            : Guid.Empty;

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
    }
}