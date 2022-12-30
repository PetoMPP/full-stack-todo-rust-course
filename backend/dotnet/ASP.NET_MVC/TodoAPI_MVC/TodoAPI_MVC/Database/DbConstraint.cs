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
            switch (binaryExpression.Left)
            {
                case BinaryExpression leftBinaryExpression:
                    ParseBinaryExpression(leftBinaryExpression);
                    break;
                case MemberExpression leftMemberExpression:
                    ParseMemberExpression(leftMemberExpression);
                    break;
                case UnaryExpression leftUnaryExpression
                when leftUnaryExpression.Operand is MemberExpression leftInnerMemberExpression:
                    ParseMemberExpression(leftInnerMemberExpression);
                    break;
                default:
                    throw new NotSupportedException(
                        $"Expression of type: '{binaryExpression.Left.GetType()}' are not supported!");
            }

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
                _ => throw new NotSupportedException(
                    $"The '{binaryExpression.NodeType}' operator in is not supported!"),
            };

            _stringBuilder.Append($" {_logicalOperator} ");

            switch (binaryExpression.Right)
            {
                case BinaryExpression rightBinaryExpression:
                    ParseBinaryExpression(rightBinaryExpression);
                    break;
                case MemberExpression rightMemberExpression:
                    ParseMemberExpression(rightMemberExpression);
                    break;
                case ConstantExpression rightConstantExpression:
                    ParseConstantExpression(rightConstantExpression);
                    break;
                case MethodCallExpression rightCallExpression:
                    ParseMethodCallExpression(rightCallExpression);
                    break;
                case UnaryExpression rightUnaryExpression
                when rightUnaryExpression.Operand is MemberExpression rightInnerMemberExpression:
                    ParseMemberExpression(rightInnerMemberExpression);
                    break;
                case UnaryExpression rightUnaryExpression
                when rightUnaryExpression.Operand is MethodCallExpression rightInnerCallExpression:
                    ParseMethodCallExpression(rightInnerCallExpression);
                    break;
                case UnaryExpression rightUnaryExpression
                when rightUnaryExpression.Operand is ConstantExpression rightInnerConstantExpression:
                    ParseConstantExpression(rightInnerConstantExpression);
                    break;
                default:
                    throw new NotSupportedException(
                        $"Expression of type: '{binaryExpression.Right.Type}' are not supported!");
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
            switch (memberExpression.Expression)
            {
                case { NodeType: ExpressionType.Parameter }:
                    if (!_dbService.TryGetSqlName(
                        memberExpression.Member, false, out var paramName, out var attributeName))
                    {
                        throw new InvalidOperationException($"Property has {attributeName}!");
                    }

                    _stringBuilder.Append(paramName);
                    break;
                case MemberExpression or ConstantExpression:
                    ParseInnerExpression(memberExpression.Expression, memberExpression.Member);
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Unable to parse {memberExpression.Expression?.GetType()}!");
            }
        }

        private void ParseInnerExpression(Expression innerExpression, MemberInfo member)
        {
            var value = member switch
            {
                PropertyInfo propertyInfo => propertyInfo.GetValue(
                    Expression.Lambda(innerExpression).Compile().DynamicInvoke()),

                FieldInfo fieldInfo => fieldInfo.GetValue(
                    Expression.Lambda(innerExpression).Compile().DynamicInvoke()),

                _ => throw new InvalidOperationException("Unable to parse inner expression!")
            };

            ValidateOperator(value);
            _stringBuilder.Append(_dbService.GetSqlValue(value));
        }

        private void ValidateOperator(object? nextValue)
        {
            if (nextValue is not null)
                return;

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
                    throw new InvalidOperationException(
                        $"Unknown operator: '{_logicalOperator}'!");
            }
        }
    }
}
