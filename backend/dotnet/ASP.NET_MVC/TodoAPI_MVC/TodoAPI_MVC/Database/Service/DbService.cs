using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using TodoAPI_MVC.Atributtes;
using TodoAPI_MVC.Database.Interfaces;
using TodoAPI_MVC.Services;

namespace TodoAPI_MVC.Database.Service
{
    public class DbService : IDbService
    {
        public record struct GetSqlNameResult(bool Ok, string? AttributeName = null);

        public DbServiceOptions Options { get; }

        public DbService(DbServiceOptions? serviceOptions = null)
        {
            Options = serviceOptions ?? new();
        }

        public string GetSqlValue(object? value)
        {
            var valueType = value?.GetType();
            if (valueType?.GetCustomAttribute<DbValueConverterAttribute>() is DbValueConverterAttribute convAttr)
            {
                var converter = (DbValueConverter)Activator.CreateInstance(convAttr.ConverterType)!;
                if (!converter.CanConvert(valueType))
                    throw new InvalidOperationException($"Converter {converter} is unable to convert from type {valueType}");

                return converter.Convert(value);
            }

            if (value is null)
                return "null";
            if (value is string or char)
                return $"'{value}'";
            if (value is DateTime dateTime)
                return $"'{dateTime.ToString(Formats.Date)}'";
            if (valueType?.IsEnum == true)
                return $"{(int)value}";

            return value.ToString() ?? string.Empty;
        }

        public bool TryGetSqlName(
            MemberInfo member,
            bool skipDefaultProperties,
            [NotNullWhen(true)] out string? sqlName,
            [NotNullWhen(false)] out string? attributeName)
        {
            sqlName = null;
            attributeName = null;

            if (member.GetCustomAttribute<DbIgnoreAttribute>() is not null)
            {
                attributeName = nameof(DbIgnoreAttribute);
                return false;
            }

            if (skipDefaultProperties && member.GetCustomAttribute<DbDefaultAttribute>() is not null)
            {
                attributeName = nameof(DbDefaultAttribute);
                return false;
            }

            sqlName = member.GetCustomAttribute<DbNameAttribute>() is DbNameAttribute nameAttribute
                ? nameAttribute.ColumnName
                : ApplyNamingPolicy(member.Name);

            return true;
        }

        private string ApplyNamingPolicy(string name) => Options.NamingPolicy.ConvertName(name);
    }

    public class DbServiceOptions
    {
        public DbNamingPolicy NamingPolicy { get; init; }

        public DbServiceOptions(DbNamingPolicy? namingPolicy = null)
        {
            NamingPolicy = namingPolicy ?? new SnakeCaseNamingPolicy();
        }
    }
}
