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
            if (conditionalExpression.Body is not BinaryExpression binaryExpression)
                throw new InvalidOperationException(
                    $"Expression should be of type {nameof(BinaryExpression)}");

            _stringBuilder = new StringBuilder();
            ParseBinaryExpression(binaryExpression);
        }

        public static implicit operator string(DbConstraint constraint) => constraint.ToSqlString();

        public string ToSqlString() => _stringBuilder.ToString();

        private void ParseBinaryExpression(BinaryExpression binaryExpression)
        {
            if (binaryExpression.Left is BinaryExpression leftBinaryExpression)
                ParseBinaryExpression(leftBinaryExpression);

            if (binaryExpression.Left is MemberExpression leftMemberExpression)
                ParseMemberExpression(leftMemberExpression);

            if (binaryExpression.Left is UnaryExpression leftUnaryExpression)
                if (leftUnaryExpression.Operand is MemberExpression leftInnerMemberExpression)
                    ParseMemberExpression(leftInnerMemberExpression);

            var logicalOperator = binaryExpression.NodeType switch
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

            if (!string.IsNullOrEmpty(logicalOperator))
                _stringBuilder.Append($" {logicalOperator} ");

            if (binaryExpression.Right is BinaryExpression rightBinaryExpression)
                ParseBinaryExpression(rightBinaryExpression);

            if (binaryExpression.Right is MemberExpression rightMemberExpression)
                ParseMemberExpression(rightMemberExpression);

            if (binaryExpression.Right is MethodCallExpression rightCallExpression)
                _stringBuilder.Append(DbHelpers.GetSqlValue(
                    Expression.Lambda(rightCallExpression).Compile().DynamicInvoke()));
        }

        private void ParseMemberExpression(MemberExpression memberExpression)
        {
            if (memberExpression.Expression?.NodeType == ExpressionType.Parameter)
            {
                if (memberExpression.Member.GetCustomAttribute<DbIgnoreAttribute>() is not null)
                    throw new InvalidOperationException("Property has DbIgnoreAttribute!");

                var paramName = memberExpression.Member.GetCustomAttribute<DbNameAttribute>() is DbNameAttribute nameAttribute
                    ? nameAttribute.ColumnName
                    : memberExpression.Member.Name;

                _stringBuilder.Append(paramName);
            }
            else if (memberExpression.Expression is MemberExpression innerMemberExpression)
            {
                var propertyInfo = (PropertyInfo)memberExpression.Member;
                var innerMemberValue = propertyInfo.GetValue(Expression.Lambda(innerMemberExpression).Compile().DynamicInvoke());
                _stringBuilder.Append(DbHelpers.GetSqlValue(innerMemberValue));
            }
            else if (memberExpression.Expression is ConstantExpression innerConstantExpression)
            {
                var fieldInfo = (FieldInfo)memberExpression.Member;
                var innerConstantValue = fieldInfo.GetValue(Expression.Lambda(innerConstantExpression).Compile().DynamicInvoke());
                _stringBuilder.Append(DbHelpers.GetSqlValue(innerConstantValue));
            }
            else
            {
                throw new InvalidOperationException("Unable to parse MemberExpression");
            }
        }
    }
}
