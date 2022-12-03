﻿using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using TodoAPI_MVC.Atributtes;
using TodoAPI_MVC.Database.Interfaces;

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
            if (value is null)
                return "null";
            if (value is string or char or Enum)
                return $"'{value}'";
            if (value is DateTime dateTime)
                return $"'{dateTime.ToString(Consts.DateFormat)}'";

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

            if (skipDefaultProperties)
            {
                if (member.GetCustomAttribute<DbDefaultAttribute>() is not null)
                {
                    attributeName = nameof(DbDefaultAttribute);
                    return false;
                }
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