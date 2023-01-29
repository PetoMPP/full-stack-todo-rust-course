using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using TodoAPI_MVC.Database.Service;

namespace TodoAPI_MVC.Database.Interfaces
{
    public interface IDbService
    {
        DbServiceOptions Options { get; }

        bool TryGetSqlName(
            MemberInfo member,
            bool skipDefaultProperties,
            [NotNullWhen(true)] out string? sqlName,
            [NotNullWhen(false)] out string? attributeName);

        string GetSqlValue(object? value);
    }
}
