using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using TodoAPI_MVC.Database.Interfaces;

namespace TodoAPI_MVC.Database
{
    public class DbConstraint
    {
        internal readonly StringBuilder _stringBuilder;
        private string _logicalOperator;
        private readonly IDbService _dbService;

        public DbConstraint(IDbService dbService, LambdaExpression conditionalExpression)
        {
            if (conditionalExpression.Body is not BinaryExpression binaryExpression)
                throw new InvalidOperationException(
                    $"Expression should be of type {nameof(BinaryExpression)}");

            _logicalOperator = "";
            _dbService = dbService;
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

            _logicalOperator = binaryExpression.NodeType switch
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

            if (!string.IsNullOrEmpty(_logicalOperator))
                _stringBuilder.Append($" {_logicalOperator} ");

            if (binaryExpression.Right is BinaryExpression rightBinaryExpression)
                ParseBinaryExpression(rightBinaryExpression);

            if (binaryExpression.Right is MemberExpression rightMemberExpression)
                ParseMemberExpression(rightMemberExpression);

            if (binaryExpression.Right is ConstantExpression rightConstantExpression)
                ParseConstantExpression(rightConstantExpression);

            if (binaryExpression.Right is MethodCallExpression rightCallExpression)
                ParseMethodCallExpression(rightCallExpression);

            if (binaryExpression.Right is UnaryExpression rightUnaryExpression)
            {
                if (rightUnaryExpression.Operand is MemberExpression rightInnerMemberExpression)
                    ParseMemberExpression(rightInnerMemberExpression);

                if (rightUnaryExpression.Operand is MethodCallExpression rightInnerCallExpression)
                    ParseMethodCallExpression(rightInnerCallExpression);
            }
        }

        private void ParseMethodCallExpression(MethodCallExpression rightCallExpression)
        {
            _stringBuilder.Append(_dbService.GetSqlValue(
                Expression.Lambda(rightCallExpression).Compile().DynamicInvoke()));
        }

        private void ParseConstantExpression(ConstantExpression constantExpression)
        {
            ValidateOperator(constantExpression.Value);
            _stringBuilder.Append(_dbService.GetSqlValue(constantExpression.Value));
        }

        private void ParseMemberExpression(MemberExpression memberExpression)
        {
            if (memberExpression.Expression?.NodeType == ExpressionType.Parameter)
            {
                if (!_dbService.TryGetSqlName(
                    memberExpression.Member, false, out var paramName, out var attributeName))
                {
                    throw new InvalidOperationException($"Property has {attributeName}!");
                }

                _stringBuilder.Append(paramName);
            }
            else if (memberExpression.Expression is MemberExpression or ConstantExpression)
            {
                ParseInnerExpression(memberExpression.Expression, memberExpression.Member);
            }
            else
            {
                throw new InvalidOperationException("Unable to parse MemberExpression!");
            }
        }

        private void ParseInnerExpression(Expression innerExpression, MemberInfo member)
        {
            object? value;
            if (member is PropertyInfo propertyInfo)
            {
                value = propertyInfo.GetValue(
                    Expression.Lambda(innerExpression).Compile().DynamicInvoke());
            }
            else if (member is FieldInfo fieldInfo)
            {
                value = fieldInfo.GetValue(
                    Expression.Lambda(innerExpression).Compile().DynamicInvoke());
            }
            else
            {
                throw new InvalidOperationException("Unable to parse inner expression!");
            }

            ValidateOperator(value);
            _stringBuilder.Append(_dbService.GetSqlValue(value));
        }

        private void ValidateOperator(object? nextValue)
        {
            if (nextValue is null)
            {
                switch (_logicalOperator)
                {
                    case "=":
                        _stringBuilder.Length -= 3;
                        _stringBuilder.Append(" is ");
                        break;
                    case "!=":
                        _stringBuilder.Length -= 4;
                        _stringBuilder.Append(" is not ");
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
