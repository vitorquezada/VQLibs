using System.Collections.Generic;
using VQLib.Util;
using VQLib.Validation;
using VQLib.Validation.Model;

namespace VQLib.Business.Model
{
    public class VQBaseResponse<T>
    {
        public VQBaseResponse()
        {
        }

        public VQBaseResponse(T data)
        {
            Data = data;
        }

        public T Data { get; set; }
        public List<VQValidationItem> Validations { get; private set; }
        public bool HasErrorValidations { get => Validations.ListHasItem(x => x.Type == VQValidationType.Error); }
        public bool HasInfoValidations { get => Validations.ListHasItem(x => x.Type == VQValidationType.Info); }
        public bool HasWarningValidations { get => Validations.ListHasItem(x => x.Type == VQValidationType.Warning); }
        public bool HasValidations { get => Validations.ListHasItem(); }

        public void AddValidationItem(string errorMessage, string propertyName, VQValidationType type = VQValidationType.Error, IEnumerable<string> messageArgs = null)
        {
            Validations.AddValidationItem(errorMessage, propertyName, type, messageArgs);
        }

        public void AddValidationResult(List<VQValidationItem> validationResult)
        {
            Validations ??= new List<VQValidationItem>();
            Validations.AddRange(validationResult);
        }
    }
}