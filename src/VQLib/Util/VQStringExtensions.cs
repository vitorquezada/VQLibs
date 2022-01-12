using System;
using System.Collections.Generic;
using System.Text.Json;

namespace VQLib.Util
{
    public static class VQStringExtensions
    {
        public static bool IsNullOrWhiteSpace(this string text) => string.IsNullOrWhiteSpace(text);

        public static bool IsNotNullOrWhiteSpace(this string text) => !string.IsNullOrWhiteSpace(text);

        public static string IsNullOrWhiteSpaceOr(this string text, string then) => !string.IsNullOrWhiteSpace(text) ? text : then;

        public static IEnumerable<string> Split(this string text, params string[] splitStrings)
        {
            if (!splitStrings.ListHasItem())
                splitStrings = new string[] { "," };

            return text.Split(splitStrings, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string ToJson<T>(this T data)
        {
            return JsonSerializer.Serialize<T>(data);
        }

        public static T JsonToObject<T>(this string value)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(value);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return default;
            }
        }
    }
}