using System;
using System.Collections.Generic;

namespace VQLibs.Util.Extension
{
    public static class VQStringExtension
    {
        public static bool IsNullOrWhiteSpace(this string text) => string.IsNullOrWhiteSpace(text);
        public static string IsNullOrWhiteSpaceOr(this string text, string then) => !string.IsNullOrWhiteSpace(text) ? text : then;

        public static IEnumerable<string> SplitByComma(this string text, string[] splitStrings = null)
        {
            if (splitStrings == null)
                splitStrings = new string[] { "," };

            return text.Split(splitStrings, StringSplitOptions.RemoveEmptyEntries);
        }

        public static IEnumerable<string> SplitCsv(this string text, string[] splitStrings = null)
        {
            if (splitStrings == null)
                splitStrings = new string[] { "," };

            return text.Split(splitStrings, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
