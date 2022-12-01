using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using TodoAPI_MVC.Atributtes;

namespace TodoAPI_MVC.Database
{
    public class DbConstraint
    {
        internal readonly StringBuilder _stringBuilder;

        public DbConstraint(LambdaExpression conditionalExpression)
        {
            if (conditionalExpression.Body is not BinaryExpression expression)
                throw new InvalidOperationException(
                    $"Expression should be of type {nameof(BinaryExpression)}");

            _stringBuilder = new StringBuilder();
            ParseExpression(expression);
        }

        public static implicit operator string(DbConstraint builder) => builder.ToSqlString();

        public string ToSqlString() => _stringBuilder.ToString();

        private void ParseExpression(BinaryExpression property)
        {
            if (property.Left is BinaryExpression left)
                ParseExpression(left);

            if (property.Left is MemberExpression leftMem)
                ParseMember(leftMem);

            if (property.Left is UnaryExpression leftUna)
            {
                if (leftUna.Operand is MemberExpression leftNullMem)
                    ParseMember(leftNullMem);
            }

            var op = property.NodeType switch
            {
                ExpressionType.Equal => "=",
                ExpressionType.GreaterThan => ">",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.LessThan => "<",
                ExpressionType.LessThanOrEqual => "<=",
                ExpressionType.NotEqual => "!=",
                ExpressionType.AndAlso => "AND",
                ExpressionType.OrElse => "OR",
                _ => "",
            };

            if (!string.IsNullOrEmpty(op))
                _stringBuilder.Append($" {op} ");

            if (property.Right is BinaryExpression right)
                ParseExpression(right);

            if (property.Right is MemberExpression rightMem)
            {
                ParseMember(rightMem);
            }
            else if (property.Right is MethodCallExpression rightCall)
            {
                var callResult = Expression.Lambda(rightCall).Compile().DynamicInvoke();
                _stringBuilder.Append(DbHelpers.GetSqlValue(callResult));
            }
        }

        private void ParseMember(MemberExpression memExp)
        {
            if (memExp.Expression?.NodeType == ExpressionType.Parameter)
            {
                if (memExp.Member.GetCustomAttribute<DbIgnoreAttribute>() is not null)
                    throw new InvalidOperationException("Property has DbIgnoreAttribute!");

                var paramName = memExp.Member.GetCustomAttribute<DbNameAttribute>() is DbNameAttribute nameAtt
                    ? nameAtt.ColumnName
                    : memExp.Member.Name;

                _stringBuilder.Append(paramName);
            }
            else if (memExp.Expression is MemberExpression innerMem)
            {
                var prop = (PropertyInfo)memExp.Member;
                var value = prop.GetValue(Expression.Lambda(innerMem).Compile().DynamicInvoke());
                _stringBuilder.Append(DbHelpers.GetSqlValue(value));
            }
            else if (memExp.Expression is ConstantExpression innerConst)
            {
                var field = (FieldInfo)memExp.Member;
                var constValue = field.GetValue(Expression.Lambda(innerConst).Compile().DynamicInvoke());
                _stringBuilder.Append(DbHelpers.GetSqlValue(constValue));
            }
        }
    }
}
