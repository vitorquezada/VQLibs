using System.Collections;
using System.Linq.Expressions;

namespace VQLib.Util
{
    public static class VQExpressionExtension
    {
        public static string GetMemberName<T>(this Expression<Func<T, object>> expression)
        {
            return GetMemberName(expression.Body);
        }

        public static string GetMemberName(this Expression expression)
        {
            var call = expression.NodeType == ExpressionType.Call
                ? (MethodCallExpression)expression
                : null;

            var member = expression.NodeType switch
            {
                ExpressionType.MemberAccess => (MemberExpression)expression,
                ExpressionType.Convert => (MemberExpression)((UnaryExpression)expression).Operand,
                ExpressionType.Parameter => null,
                ExpressionType.Call => null,
                _ => throw new NotSupportedException(expression.NodeType.ToString()),
            };

            string name;

            if (call != null
                && typeof(IEnumerable).IsAssignableFrom(call.Method.DeclaringType)
                && call.Method.Name == "get_Item")
            {
                var field = (MemberExpression)call.Arguments[0];
                var lambda = Expression.Lambda(field);
                var fn = lambda.Compile();
                var index = fn.DynamicInvoke();

                name = $"{GetMemberName(call.Object)}[{index}]";
            }
            else
            {
                name = member == null
                    ? string.Empty
                    : $"{GetMemberName(member.Expression)}.{member.Member.Name}";
            }

            if (name.StartsWith("."))
                name = name[1..];

            return name;
        }
    }
}