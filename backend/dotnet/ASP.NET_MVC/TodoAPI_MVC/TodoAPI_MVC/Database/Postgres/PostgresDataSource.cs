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

        private record struct PropertyData(PropertyInfo PropertyInfo, string SqlName);

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
            var properties = typeof(T).GetProperties().Where(p => p.CanRead && p.CanWrite);
            var propertiesData = GetPropertiesData(properties, true);

            if (!propertiesData.Any())
                throw new ArgumentException("Type doesn't have any readable properties!", nameof(T));

            using var command = _npgsqlDataSource.CreateCommand(commandString);
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await ReadValues<T>(reader, propertiesData, cancellationToken);
        }

        public async Task<int> DeleteRows(
            string tableName,
            string? sqlFilter = null,
            CancellationToken cancellationToken = default)
        {
            var commandString = CreateDeleteCommandString(tableName, sqlFilter);
            using var command = _npgsqlDataSource.CreateCommand(commandString);
            return await command.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task<int> UpdateRows<T>(
            string tableName,
            T value,
            string? sqlFilter = null,
            CancellationToken cancellationToken = default)
        {
            var properties = typeof(T).GetProperties().Where(p => p.CanRead);
            var propertiesData = GetPropertiesData(properties, true);

            if (!propertiesData.Any())
                throw new ArgumentException("Type doesn't have any readable properties!", nameof(T));

            var columnNames = propertiesData.Select(p => p.SqlName);
            var commandString = CreateUpdateCommandString(tableName, propertiesData, value, sqlFilter, false);

            using var command = _npgsqlDataSource.CreateCommand(commandString);
            return await command.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task<IList<T>> UpdateRowsReturning<T>(
            string tableName,
            T value,
            string? sqlFilter = null,
            CancellationToken cancellationToken = default)
        {
            var properties = typeof(T).GetProperties().Where(p => p.CanRead && p.CanWrite);
            var propertiesData = GetPropertiesData(properties, true);

            if (!propertiesData.Any())
                throw new ArgumentException("Type doesn't have any readable properties!", nameof(T));

            var columnNames = propertiesData.Select(p => p.SqlName);
            var commandString = CreateUpdateCommandString(tableName, propertiesData, value, sqlFilter, true);

            using var command = _npgsqlDataSource.CreateCommand(commandString);
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await ReadValues<T>(reader, propertiesData, cancellationToken);
        }

        public async Task<IList<T>> InsertRowsReturning<T>(
            string tableName,
            IEnumerable<T> values,
            CancellationToken cancellationToken = default)
        {
            var properties = typeof(T).GetProperties().Where(p => p.CanRead && p.CanWrite);
            var propertiesData = GetPropertiesData(properties, true);

            if (!propertiesData.Any())
                throw new ArgumentException("Type doesn't have any readable properties!", nameof(T));

            var columnNames = propertiesData.Select(p => p.SqlName);
            var commandString = CreateInsertCommandString(tableName, propertiesData, values, true);

            using var command = _npgsqlDataSource.CreateCommand(commandString);
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await ReadValues<T>(reader, propertiesData, cancellationToken);
        }

        public async Task<int> InsertRows<T>(
            string tableName,
            IEnumerable<T> values,
            CancellationToken cancellationToken = default)
        {
            var properties = typeof(T).GetProperties().Where(p => p.CanRead);
            var propertiesData = GetPropertiesData(properties, true);

            if (!propertiesData.Any())
                throw new ArgumentException("Type doesn't have any readable properties!", nameof(T));

            var columnNames = propertiesData.Select(p => p.SqlName);
            var commandString = CreateInsertCommandString(tableName, propertiesData, values, false);

            using var command = _npgsqlDataSource.CreateCommand(commandString);
            return await command.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task<IList<T>> ReadRows<T>(
            string tableName,
            string? sqlFilter = null,
            CancellationToken cancellationToken = default)
        {
            var properties = typeof(T).GetProperties().Where(p => p.CanWrite);
            var propertiesData = GetPropertiesData(properties, false);

            if (!propertiesData.Any())
                throw new ArgumentException("Type doesn't have any writeable properties!", nameof(T));

            var columnNames = propertiesData.Select(p => p.SqlName);
            var commandString = CreateSelectCommandString(tableName, sqlFilter, columnNames);

            using var command = _npgsqlDataSource.CreateCommand(commandString);
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await ReadValues<T>(reader, propertiesData, cancellationToken);
        }

        private static async Task<IList<T>> ReadValues<T>(
            NpgsqlDataReader reader,
            IEnumerable<PropertyData> propertiesData,
            CancellationToken cancellationToken)
        {
            var columnSchema = await reader.GetColumnSchemaAsync(cancellationToken);
            var propertyDataMap = MapPropertiesToColumnsSchema(propertiesData, columnSchema);

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
            var command = $"DELETE FROM {tableName}";
            if (sqlFilter is not null)
                command = $"{command} WHERE {sqlFilter}";

            return command;
        }

        private string CreateUpdateCommandString<T>(
            string tableName, IEnumerable<PropertyData> propertiesData, T value, string? sqlFilter, bool returning)
        {
            var builder = new StringBuilder();
            var setterString = string.Join(
                ", ", propertiesData.Select(p =>
                    $"{p.SqlName} = {_dbService.GetSqlValue(p.PropertyInfo.GetValue(value))}"));

            builder.Append("UPDATE ").Append(tableName).Append(" SET ").Append(setterString);

            if (sqlFilter is not null)
                builder.Append(" WHERE ").Append(sqlFilter);

            if (returning)
                builder.Append(" RETURNING ").AppendJoin(", ", propertiesData.Select(p => p.SqlName));

            return builder.ToString();
        }

        private string CreateInsertCommandString<T>(
            string tableName, IEnumerable<PropertyData> propertiesData, IEnumerable<T> values, bool returning)
        {
            var columnNames = propertiesData.Select(p => p.SqlName);
            var builder = new StringBuilder();
            builder.Append("INSERT INTO ").Append(tableName).Append(" (").AppendJoin(", ", columnNames).Append(") ");
            builder.Append("VALUES ");

            foreach (var value in values)
                builder.Append('(').AppendJoin(", ", propertiesData.Select(p => _dbService.GetSqlValue(p.PropertyInfo.GetValue(value)))).Append("),");

            builder.Length--;

            if (returning)
                builder.Append(" RETURNING ").AppendJoin(", ", columnNames);

            return builder.ToString();
        }

        private IEnumerable<PropertyData> GetPropertiesData(
            IEnumerable<PropertyInfo> properties, bool skipDefaultProperties)
        {
            foreach (var property in properties)
            {
                if (!_dbService.TryGetSqlName(property, skipDefaultProperties, out var sqlName, out _))
                    continue;

                yield return new PropertyData(property, sqlName);
            }
        }

        private static string CreateSelectCommandString(
            string tableName, string? sqlFilter, IEnumerable<string> columnNames)
        {
            sqlFilter = sqlFilter is null ? string.Empty : $" WHERE {sqlFilter}";
            return $"SELECT {string.Join(", ", columnNames)} FROM {tableName}{sqlFilter}";
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
