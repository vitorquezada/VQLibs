using System.Collections.Generic;
using System.Linq;
using VQLibs.Validation.Enum;
using VQLibs.Validation.Exceptions;

namespace VQLibs.Validation.Model
{
    public class VQValidations
    {
        public bool HasAnyValidation { get => HasValidations(); }
        public bool HasError { get => HasValidations(VQValidationType.Error); }
        public bool HasInfo { get => HasValidations(VQValidationType.Info); }

        public List<VQValidationItem> ErrorList { get => GetValidationByType(VQValidationType.Error); }
        public List<VQValidationItem> InfoList { get => GetValidationByType(VQValidationType.Info); }

        public List<VQValidationItem> ValidationList { get; set; } = new List<VQValidationItem>();

        public void AddValidation(string field, string message, VQValidationType type = VQValidationType.Error)
        {
            if (ValidationList == null)
                ValidationList = new List<VQValidationItem>();

            ValidationList.Add(new VQValidationItem(field, message, type));
        }

        public void AssertValid()
        {
            if (HasAnyValidation)
                throw new VQValidationException(this);
        }

        public void AssertValid(VQValidationType typeToConsider)
        {
            if (HasValidations(typeToConsider))
                throw new VQValidationException(new VQValidations
                {
                    ValidationList = GetValidationByType(typeToConsider)
                });
        }

        #region " PRIVATE METHODS "

        private bool HasValidations(VQValidationType? x = null) =>
            ValidationList != null && (x.HasValue ? ValidationList.Any(i => i.Type == x.Value) : ValidationList.Any());

        private List<VQValidationItem> GetValidationByType(VQValidationType type) =>
            ValidationList?.Where(x => x.Type == type).ToList() ?? new List<VQValidationItem>();

        #endregion
    }
}
