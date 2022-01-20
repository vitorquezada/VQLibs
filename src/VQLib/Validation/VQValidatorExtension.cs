using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using VQLib.Util;
using VQLib.Validation.Model;

namespace VQLib.Validation
{
    public static class ValidatorExtensions
    {
        public static List<VQValidationItem> AddValidationItem(
            this List<VQValidationItem> validationItems,
            string errorMessage,
            string propertyName,
            VQValidationType type = VQValidationType.Error,
            IEnumerable<string> messageArgs = null)
        {
            validationItems.Add(new VQValidationItem
            {
                Message = new VQValidationItemMessage(errorMessage, messageArgs),
                PropertyName = propertyName,
                Type = type
            });

            return validationItems;
        }

        public static string GetPropertyName(this string propertyName, string prefixPropertyName = null, string sufixPropertyName = null)
        {
            return $"{prefixPropertyName ?? string.Empty}{propertyName ?? string.Empty}{sufixPropertyName ?? string.Empty}";
        }

        public static string GetPropertyName<T>(Expression<Func<T, object>> propertyName, string prefixPropertyName = null, string sufixPropertyName = null)
        {
            return $"{prefixPropertyName ?? string.Empty}{propertyName.GetMemberName() ?? string.Empty}{sufixPropertyName ?? string.Empty}";
        }
    }
}