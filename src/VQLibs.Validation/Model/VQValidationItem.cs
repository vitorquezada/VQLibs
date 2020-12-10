using VQLibs.Validation.Enum;

namespace VQLibs.Validation.Model
{
    public class VQValidationItem
    {
        public VQValidationItem()
        {

        }

        public VQValidationItem(string field, string message, VQValidationType type)
        {
            Field = field;
            Message = message;
            Type = type;
        }

        public string Field { get; set; }
        public string Message { get; set; }
        public VQValidationType Type { get; set; }
    }
}
