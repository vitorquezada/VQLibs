using System;
using System.Linq;
using VQLibs.Validation.Model;

namespace VQLibs.Validation.Exceptions
{
    public class VQValidationException : Exception
    {
        private const string _4_SPACES = "    ";
        private const string _8_SPACES = "        ";

        public VQValidations Validations { get; set; }

        public VQValidationException(VQValidations validations)
        {
            Validations = validations;
        }

        public VQValidationException(VQValidations validations, Exception innerException) : base(null, innerException)
        {
            Validations = validations;
        }

        public override string ToString()
        {
            var val = Validations.ValidationList.OrderBy(x => x.Type).ThenBy(x => x.Field).ThenBy(x => x.Message);

            var validationMsg = string.Join($"{Environment.NewLine}{_8_SPACES}", val.Select(x => $"{{ {x.Type}, {x.Field}, {x.Message} }}"));

            var s = $"VQValidation Exception: Validation is rejected.{Environment.NewLine}";
            s += $"{_4_SPACES}- Validations:{validationMsg}{Environment.NewLine}";
            s += base.ToString();
            return s;
        }
    }
}
