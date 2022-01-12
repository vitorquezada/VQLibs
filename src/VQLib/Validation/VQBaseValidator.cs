using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VQLib.Util;
using VQLib.Validation.Model;

namespace VQLib.Validation
{
    public abstract class VQBaseValidator<T>
    {
        public abstract Task<List<VQValidationItem>> Validate(T model, string prefixPropertyName = null, string sufixPropertyName = null);

        public virtual List<VQValidationItem> ValidateSync(T model, string prefixPropertyName = null, string sufixPropertyName = null)
        {
            return Validate(model, prefixPropertyName, sufixPropertyName).ConfigureAwait(true).GetAwaiter().GetResult();
        }

        public string GetPropertyName(Expression<Func<T, object>> model, string prefixPropertyName = null, string sufixPropertyName = null)
        {
            var name = model.GetMemberName();
            return name.GetPropertyName(prefixPropertyName, sufixPropertyName);
        }

        public void AddValdationItem(List<VQValidationItem> result, string message, Expression<Func<T, object>> propName, string prefixPropertyName = null, string sufixPropertyName = null, IEnumerable<string> messageArgs = null, VQValidationType type = VQValidationType.Error)
        {
            result.AddValidationItem(
                message,
                GetPropertyName(propName).GetPropertyName(prefixPropertyName, sufixPropertyName),
                type,
                messageArgs: messageArgs);
        }
    }
}