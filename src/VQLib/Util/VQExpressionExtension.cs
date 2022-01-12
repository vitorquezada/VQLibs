using System;
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
            var member = expression.NodeType switch
            {
                ExpressionType.MemberAccess => (MemberExpression)expression,
                ExpressionType.Convert => (MemberExpression)((UnaryExpression)expression).Operand,
                ExpressionType.Parameter => null,
                _ => throw new NotSupportedException(expression.NodeType.ToString()),
            };

            var name = member == null
                ? string.Empty
                : $"{GetMemberName(member.Expression)}.{member.Member.Name}";

            if (name.StartsWith("."))
                name = name[1..];

            return name;
        }
    }
}