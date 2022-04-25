using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using VQLib.Util;
using VQLib.Validation;
using VQLib.Validation.Model;

namespace VQLib.Business.Model
{
    public class VQBaseResponse<T> : VQBaseResponse
    {
        public VQBaseResponse() : base()
        {
        }

        public VQBaseResponse(T data)
        {
            Data = data;
        }

        public T Data { get; set; }

        public string GetPropertyName(Expression<Func<T, object>> property, string prefix = null, string sufix = null)
            => $"{prefix ?? string.Empty}{property.GetMemberName()}{sufix ?? string.Empty}";
    }

    public class VQBaseResponse
    {
        public VQBaseResponse()
        {
        }

        public List<VQValidationItem> Validations { get; private set; } = new List<VQValidationItem>();

        public bool HasErrorValidations { get => Validations.ListHasItem(x => x.Type == VQValidationType.Error); }

        public bool HasInfoValidations { get => Validations.ListHasItem(x => x.Type == VQValidationType.Info); }

        public bool HasWarningValidations { get => Validations.ListHasItem(x => x.Type == VQValidationType.Warning); }

        public bool HasValidations { get => Validations.ListHasItem(); }

        public void AddValidationItem(string errorMessage, string propertyName, VQValidationType type = VQValidationType.Error, IEnumerable<string> messageArgs = null)
        {
            Validations ??= new List<VQValidationItem>();
            Validations.AddValidationItem(errorMessage, propertyName, type, messageArgs);
        }

        public void AddValidationResult(List<VQValidationItem> validationResult)
        {
            Validations ??= new List<VQValidationItem>();

            if (validationResult.ListHasItem())
                Validations.AddRange(validationResult);
        }

        public void AddValidationResult(ValidationResult validationResult)
        {
            Validations ??= new List<VQValidationItem>();

            if (validationResult != null && validationResult.Errors != null && validationResult.Errors.Any())
                Validations.AddRange(validationResult.Errors.Select(x => new VQValidationItem
                {
                    PropertyName = x.PropertyName,
                    Type = x.Severity switch
                    {
                        Severity.Error => VQValidationType.Error,
                        Severity.Warning => VQValidationType.Warning,
                        Severity.Info => VQValidationType.Info,
                        _ => throw new ArgumentOutOfRangeException(nameof(x.Severity)),
                    },
                    Message = new VQValidationItemMessage(x.ErrorMessage)
                }));
        }
    }
}