﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

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

        public static T FromJson<T>(this string value)
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

        public static bool EmailIsValid(this string value)
        {
            const string emailPattern = @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$";

            return !value.IsNullOrWhiteSpace() && Regex.IsMatch(value.Trim(), emailPattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
        }

        public static string OnlyNumbers(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return Regex.Replace(text, @"[^0-9]+", string.Empty);
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
    }
}