using System.Collections.Generic;
using System.Linq;

namespace VQLib.Validation.Model
{
    public class VQValidationItem
    {
        public string PropertyName { get; set; }
        public VQValidationType Type { get; set; }
        public VQValidationItemMessage Message { get; set; }
    }

    public class VQValidationItemMessage
    {
        public VQValidationItemMessage()
        {
        }

        public VQValidationItemMessage(string message, IEnumerable<string> args = null)
        {
            Message = message;
            if (args != null)
                Args = args.ToList();
        }

        public string Message { get; set; }

        public List<string> Args { get; set; } = new List<string>();
    }

    public enum VQValidationType
    {
        Error = 0,
        Warning = 1,
        Info = 2,
    }
}