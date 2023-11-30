using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using VQLib.Util.JsonConverters;

namespace VQLib.Util
{
    public static class VQStringExtensions
    {
        public static JsonSerializerOptions VQDefaultJsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Converters =
            {
                new DateOnlyJsonConverter(),
            }
        };

        public static bool CnpjIsValid(this string cnpj)
        {
            int[] multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int soma;
            int resto;
            string digito;
            string tempCnpj;

            cnpj = cnpj.OnlyNumbers();
            if (cnpj.Length != 14)
                return false;

            if (cnpj.All(x => x == cnpj[0]))
                return false;

            tempCnpj = cnpj.Substring(0, 12);
            soma = 0;
            for (int i = 0; i < 12; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];
            resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = resto.ToString();
            tempCnpj += digito;
            soma = 0;
            for (int i = 0; i < 13; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];
            resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito += resto.ToString();

            return cnpj.EndsWith(digito);
        }

        public static bool ContainsIgnoreCaseAndAccents(this string x1, string x2)
        {
            return RemoveAccents(x1).Contains(RemoveAccents(x2), StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool CpfIsValid(this string cpf)
        {
            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tempCpf;
            string digito;
            int soma;
            int resto;

            cpf = cpf.OnlyNumbers();
            if (cpf.Length != 11)
                return false;

            if (cpf.All(x => x == cpf[0]))
                return false;

            tempCpf = cpf.Substring(0, 9);
            soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = resto.ToString();
            tempCpf = tempCpf + digito;
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito += resto.ToString();

            return cpf.EndsWith(digito);
        }

        public static bool EmailIsValid(this string value)
        {
            const string emailPattern = @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$";

            return !value.IsNullOrWhiteSpace() && Regex.IsMatch(value.Trim(), emailPattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
        }

        public static bool EqualsIgnoreCaseAndAccents(this string x1, string x2)
        {
            return RemoveAccents(x1).Equals(RemoveAccents(x2), StringComparison.CurrentCultureIgnoreCase);
        }

        public static string FormatCNPJ(this string CNPJ)
        {
            if (!CNPJ.CnpjIsValid()) throw new FormatException($"CNPJ \"{CNPJ}\" is not valid.");

            return Convert.ToUInt64(CNPJ.OnlyNumbers()).ToString("00'.'000'.'000'/'0000'-'00");
        }

        public static string FormatCPF(this string CPF)
        {
            if (!CPF.CpfIsValid()) throw new FormatException($"CPF \"{CPF}\" is not valid.");

            return Convert.ToUInt64(CPF.OnlyNumbers()).ToString("000'.'000'.'000'-'00");
        }

        public static T? FromJson<T>(this string? value, JsonSerializerOptions? options = null)
        {
            if (string.IsNullOrWhiteSpace(value))
                return default(T);

            return JsonSerializer.Deserialize<T>(value, options ?? VQDefaultJsonOptions);
        }

        public static async Task<T?> FromJson<T>(this Stream? value, JsonSerializerOptions? options = null)
        {
            if (value == null || value.Length <= 0)
                return default(T);
            return await JsonSerializer.DeserializeAsync<T>(value, options ?? VQDefaultJsonOptions);
        }

        public static string GetDescription<T>(this T value) where T : struct
        {
            var valueName = value.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(valueName)) return string.Empty;
            return typeof(T).GetField(valueName)?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? valueName;
        }

        public static string GetMaskedEmail(this string email)
        {
            const string group1 = @"(?<=[\w]{1})[\S]*(?=[\w]{1}@)";
            const string group2 = @"(?<=@[\w]{1})[\S]*?(?=[\w]{1}\.)";

            if (email.IsNullOrWhiteSpace())
                return email;

            return Regex.Replace(Regex.Replace(email, group1, m => new string('*', m.Length)), group2, m => new string('*', m.Length));
        }

        public static bool IsNotNullOrWhiteSpace([NotNullWhen(true)] this string? text) => !string.IsNullOrWhiteSpace(text);

        public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? text) => string.IsNullOrWhiteSpace(text);

        public static string? IsNullOrWhiteSpaceOr(this string? text, string? then) => !string.IsNullOrWhiteSpace(text) ? text : then;

        public static string? OnlyNumbers(this string? text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return Regex.Replace(text, @"[^0-9]+", string.Empty);
        }

        public static string RemoveAccents(this string text)
        {
            string formD = text.Normalize(NormalizationForm.FormD);
            string newText = string.Empty;

            foreach (char ch in formD)
            {
                var charCategory = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (charCategory != UnicodeCategory.NonSpacingMark)
                {
                    newText += ch;
                }
            }

            return newText.Normalize(NormalizationForm.FormC);
        }

        public static IEnumerable<string> Split(this string text, params string[] splitStrings)
        {
            if (!splitStrings.ListHasItem())
                splitStrings = new string[] { "," };

            return text.Split(splitStrings, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string ToJson<T>(this T data, JsonSerializerOptions? options = null)
        {
            return JsonSerializer.Serialize<T>(data, options ?? VQDefaultJsonOptions);
        }
    }
}