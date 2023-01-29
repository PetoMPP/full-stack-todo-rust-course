using Npgsql;
using Npgsql.Schema;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using TodoAPI_MVC.Database.Interfaces;

namespace TodoAPI_MVC.Database.Postgres
{
    public interface IPostgresDataSource
    {
        Task<int> DeleteRows(string tableName, string? sqlFilter = null, CancellationToken cancellationToken = default);
        Task<int> Execute(string commandString, CancellationToken cancellationToken = default);
        Task<IList<T>> ExecuteQuery<T>(string commandString, CancellationToken cancellationToken = default);
        Task<int> InsertRows<T>(string tableName, IEnumerable<T> values, CancellationToken cancellationToken = default);
        Task<IList<T>> InsertRowsReturning<T>(string tableName, IEnumerable<T> values, CancellationToken cancellationToken = default);
        Task<IList<T>> ReadRows<T>(string tableName, string? sqlFilter = null, CancellationToken cancellationToken = default);
        Task<int> UpdateRows<T>(string tableName, T value, string? sqlFilter = null, CancellationToken cancellationToken = default);
        Task<IList<T>> UpdateRowsReturning<T>(string tableName, T value, string? sqlFilter = null, CancellationToken cancellationToken = default);
    }

    public class PostgresDataSource : IPostgresDataSource
    {
        private readonly NpgsqlDataSource _npgsqlDataSource;
        private readonly IDbService _dbService;
        private readonly Dictionary<Type, PropertiesData> _typeData = new();

        private record struct PropertyData(PropertyInfo PropertyInfo, string SqlName);
        private record struct PropertiesData(PropertyData[] WriteableProperties, PropertyData[] ReadableProperties)
        {
            public bool Any() => WriteableProperties.Any() || ReadableProperties.Any();
        }

        public PostgresDataSource(NpgsqlDataSource npgsqlDataSource, IDbService dbService)
        {
            _npgsqlDataSource = npgsqlDataSource;
            _dbService = dbService;
        }

        public async Task<int> Execute(string commandString, CancellationToken cancellationToken)
        {
            using var command = _npgsqlDataSource.CreateCommand(commandString);
            return await command.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task<IList<T>> ExecuteQuery<T>(string commandString, CancellationToken cancellationToken)
        {
            using var command = _npgsqlDataSource.CreateCommand(commandString);
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await ReadValues<T>(reader, cancellationToken);
        }

        public async Task<int> DeleteRows(
            string tableName,
            string? sqlFilter = null,
            CancellationToken cancellationToken = default)
        {
            using var command = _npgsqlDataSource.CreateCommand(
                CreateDeleteCommandString(tableName, sqlFilter));
            return await command.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task<int> UpdateRows<T>(
            string tableName,
            T value,
            string? sqlFilter = null,
            CancellationToken cancellationToken = default)
        {
            using var command = _npgsqlDataSource.CreateCommand(
                CreateUpdateCommandString(tableName, value, sqlFilter, false));
            return await command.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task<IList<T>> UpdateRowsReturning<T>(
            string tableName,
            T value,
            string? sqlFilter = null,
            CancellationToken cancellationToken = default)
        {
            using var command = _npgsqlDataSource.CreateCommand(
                CreateUpdateCommandString(tableName, value, sqlFilter, true));
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await ReadValues<T>(reader, cancellationToken);
        }

        public async Task<IList<T>> InsertRowsReturning<T>(
            string tableName,
            IEnumerable<T> values,
            CancellationToken cancellationToken = default)
        {
            using var command = _npgsqlDataSource.CreateCommand(
                CreateInsertCommandString(tableName, values, true));
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await ReadValues<T>(reader, cancellationToken);
        }

        public async Task<int> InsertRows<T>(
            string tableName,
            IEnumerable<T> values,
            CancellationToken cancellationToken = default)
        {
            using var command = _npgsqlDataSource.CreateCommand(
                CreateInsertCommandString(tableName, values, false));
            return await command.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task<IList<T>> ReadRows<T>(
            string tableName,
            string? sqlFilter = null,
            CancellationToken cancellationToken = default)
        {
            using var command = _npgsqlDataSource.CreateCommand(
                CreateSelectCommandString<T>(tableName, sqlFilter));
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await ReadValues<T>(reader, cancellationToken);
        }

        private async Task<IList<T>> ReadValues<T>(
            NpgsqlDataReader reader,
            CancellationToken cancellationToken)
        {
            var propertiesData = GetPropertiesData<T>();
            var columnSchema = await reader.GetColumnSchemaAsync(cancellationToken);
            var propertyDataMap = MapPropertiesToColumnsSchema(propertiesData.ReadableProperties, columnSchema);

            var result = new List<T>();

            while (await reader.ReadAsync(cancellationToken))
            {
                object item = Activator.CreateInstance<T>()!;

                for (var i = 0; i < columnSchema.Count; i++)
                {
                    if (propertyDataMap.TryGetValue(i, out var propertyData))
                    {
                        var value = reader.GetValue(i);
                        var propertyType = Nullable.GetUnderlyingType(propertyData.PropertyInfo.PropertyType)
                            ?? propertyData.PropertyInfo.PropertyType;

                        if (value.GetType().IsAssignableTo(propertyData.PropertyInfo.PropertyType))
                            propertyData.PropertyInfo.SetValue(item, value);

                        if (propertyType.IsEnum && value is string stringValue &&
                            Enum.TryParse(propertyType, stringValue, out var enumValue))
                        {
                            propertyData.PropertyInfo.SetValue(item, enumValue);
                        }

                        if (propertyType.IsEnum && value is int intValue)
                            propertyData.PropertyInfo.SetValue(item, intValue);
                    }
                }

                result.Add((T)item);
            }

            return result;
        }

        private static string CreateDeleteCommandString(string tableName, string? sqlFilter)
        {
            var builder = new StringBuilder();
            builder
                .Append("DELETE FROM ")
                .Append(tableName);

            if (!string.IsNullOrEmpty(sqlFilter))
            {
                builder
                    .Append(" WHERE ")
                    .Append(sqlFilter);
            }

            return builder.ToString();
        }

        private string CreateUpdateCommandString<T>(
            string tableName, T value, string? sqlFilter, bool returning)
        {
            var propertiesData = GetPropertiesData<T>();
            if (!propertiesData.Any())
                throw new ArgumentException("Type doesn't have any readable properties!", nameof(T));

            var builder = new StringBuilder();

            builder
                .Append("UPDATE ")
                .Append(tableName)
                .Append(" SET ")
                .AppendJoin(
                    ", ",
                    propertiesData.WriteableProperties
                        .Select(p => $"{p.SqlName} = {_dbService.GetSqlValue(p.PropertyInfo.GetValue(value))}"));

            if (sqlFilter is not null)
            {
                builder
                    .Append(" WHERE ")
                    .Append(sqlFilter);
            }

            if (returning)
            {
                builder
                    .Append(" RETURNING ")
                    .AppendJoin(", ", propertiesData.ReadableProperties.Select(p => p.SqlName));
            }

            return builder.ToString();
        }

        private string CreateInsertCommandString<T>(
            string tableName, IEnumerable<T> values, bool returning)
        {
            var propertiesData = GetPropertiesData<T>();
            if (!propertiesData.Any())
                throw new ArgumentException("Type doesn't have any readable properties!", nameof(T));

            var builder = new StringBuilder();
            builder
                .Append("INSERT INTO ")
                .Append(tableName)
                .Append(" (")
                .AppendJoin(", ", propertiesData.WriteableProperties.Select(p => p.SqlName))
                .Append(") ")
                .Append("VALUES ");

            foreach (var value in values)
            {
                builder
                    .Append('(')
                    .AppendJoin(", ", propertiesData.WriteableProperties
                        .Select(p => _dbService.GetSqlValue(p.PropertyInfo.GetValue(value))))
                    .Append("),");
            }

            builder.Length--;

            if (returning)
            {
                builder
                    .Append(" RETURNING ")
                    .AppendJoin(", ", propertiesData.ReadableProperties.Select(p => p.SqlName));
            }

            return builder.ToString();
        }

        private string CreateSelectCommandString<T>(
            string tableName, string? sqlFilter)
        {
            var propertiesData = GetPropertiesData<T>();
            if (!propertiesData.Any())
                throw new ArgumentException("Type doesn't have any readable properties!", nameof(T));

            var builder = new StringBuilder();
            builder
                .Append("SELECT ")
                .AppendJoin(", ", propertiesData.ReadableProperties.Select(p => p.SqlName))
                .Append(" FROM ")
                .Append(tableName);

            if (!string.IsNullOrEmpty(sqlFilter))
            {
                builder
                    .Append(" WHERE ")
                    .Append(sqlFilter);
            }

            return builder.ToString();
        }

        private PropertiesData GetPropertiesData<T>()
        {
            if (_typeData.TryGetValue(typeof(T), out var typeProperties))
                return typeProperties;

            var readable = new List<PropertyData>();
            var writeable = new List<PropertyData>();
            foreach (var property in typeof(T).GetProperties().Where(p => p.CanRead))
            {
                if (!_dbService.TryGetSqlName(property, false, out var sqlName, out _))
                    continue;

                readable.Add(new PropertyData(property, sqlName));
            }

            foreach (var property in typeof(T).GetProperties().Where(p => p.CanWrite))
            {
                if (!_dbService.TryGetSqlName(property, true, out var sqlName, out _))
                    continue;

                writeable.Add(new PropertyData(property, sqlName));
            }

            var result = new PropertiesData(writeable.ToArray(), readable.ToArray());
            _typeData.Add(typeof(T), result);

            return result;
        }

        private static Dictionary<int, PropertyData> MapPropertiesToColumnsSchema(
            IEnumerable<PropertyData> propertiesData, ReadOnlyCollection<NpgsqlDbColumn> columnSchema)
        {
            var result = new Dictionary<int, PropertyData>();
            foreach (var propertyData in propertiesData)
            {
                if (columnSchema.FirstOrDefault(c => CompareNames(c, propertyData)) is NpgsqlDbColumn column &&
                    column.ColumnOrdinal is int ordinal)
                {
                    result[ordinal] = propertyData;
                }
            }

            return result;

            static bool CompareNames(NpgsqlDbColumn col, PropertyData propertyData)
            {
                return string.Equals(col.ColumnName, propertyData.SqlName, StringComparison.InvariantCultureIgnoreCase);
            }
        }
    }
}
