using Npgsql;
using Npgsql.Schema;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using TodoAPI_MVC.Atributtes;

namespace TodoAPI_MVC.Database
{
    public interface IPostgresDataSource
    {
        NpgsqlDataSource NpgsqlDataSource { get; }

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
        private record struct PropertyData(PropertyInfo PropertyInfo, string SqlName);

        public NpgsqlDataSource NpgsqlDataSource { get; }

        public PostgresDataSource(NpgsqlDataSource npgsqlDataSource)
        {
            NpgsqlDataSource = npgsqlDataSource;
        }

        public async Task<int> Execute(string commandString, CancellationToken cancellationToken)
        {
            using var command = NpgsqlDataSource.CreateCommand(commandString);
            return await command.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task<IList<T>> ExecuteQuery<T>(string commandString, CancellationToken cancellationToken)
        {
            var properties = typeof(T).GetProperties().Where(p => p.CanRead && p.CanWrite);
            var propertiesData = GetPropertiesData(properties, true);

            if (!propertiesData.Any())
                throw new ArgumentException("Type doesn't have any readable properties!", nameof(T));

            using var command = NpgsqlDataSource.CreateCommand(commandString);
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await ReadValues<T>(reader, propertiesData, cancellationToken);
        }

        public async Task<int> DeleteRows(
            string tableName,
            string? sqlFilter = null,
            CancellationToken cancellationToken = default)
        {
            var commandString = CreateDeleteCommandString(tableName, sqlFilter);
            using var command = NpgsqlDataSource.CreateCommand(commandString);
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

            using var command = NpgsqlDataSource.CreateCommand(commandString);
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

            using var command = NpgsqlDataSource.CreateCommand(commandString);
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

            using var command = NpgsqlDataSource.CreateCommand(commandString);
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

            using var command = NpgsqlDataSource.CreateCommand(commandString);
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

            using var command = NpgsqlDataSource.CreateCommand(commandString);
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

                        else if (propertyType.IsEnum && value is string stringValue)
                            if (Enum.TryParse(propertyType, stringValue, out var enumValue))
                                propertyData.PropertyInfo.SetValue(item, enumValue);
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

        private static string CreateUpdateCommandString<T>(
            string tableName, IEnumerable<PropertyData> propertiesData, T value, string? sqlFilter, bool returning)
        {
            var builder = new StringBuilder();
            var setterString = string.Join(", ", propertiesData.Select(p => $"{p.SqlName} = {DbHelpers.GetSqlValue(p.PropertyInfo.GetValue(value))}"));
            builder.Append($"UPDATE {tableName} SET {setterString}");

            if (sqlFilter is not null)
                builder.Append($" WHERE {sqlFilter}");

            if (returning)
                builder.Append($" RETURNING {string.Join(", ", propertiesData.Select(p => p.SqlName))}");

            return builder.ToString();
        }

        private static string CreateInsertCommandString<T>(
            string tableName, IEnumerable<PropertyData> propertiesData, IEnumerable<T> values, bool returning)
        {
            var columnNames = propertiesData.Select(p => p.SqlName);
            var builder = new StringBuilder();
            builder.Append($"INSERT INTO {tableName} ({string.Join(", ", columnNames)}) ");
            builder.Append("VALUES ");

            foreach (var value in values)
                builder.Append($"({string.Join(", ", propertiesData.Select(p => DbHelpers.GetSqlValue(p.PropertyInfo.GetValue(value))))}),");

            builder.Length--;

            if (returning)
                builder.Append($" RETURNING {string.Join(", ", columnNames)}");

            return builder.ToString();
        }

        private static IEnumerable<PropertyData> GetPropertiesData(
            IEnumerable<PropertyInfo> properties, bool skipDefaultProperties)
        {
            foreach (var property in properties)
            {
                if (property.GetCustomAttribute<DbIgnoreAttribute>() is not null)
                    continue;

                if (skipDefaultProperties)
                    if (property.GetCustomAttribute<DbDefaultAttribute>() is not null)
                        continue;

                if (property.GetCustomAttribute<DbNameAttribute>() is DbNameAttribute attribute &&
                    !string.IsNullOrWhiteSpace(attribute.ColumnName))
                {
                    yield return new PropertyData(property, attribute.ColumnName);
                    continue;
                }

                yield return new PropertyData(property, property.Name);
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
                    result[ordinal] = propertyData;
            }

            return result;

            static bool CompareNames(NpgsqlDbColumn col, PropertyData propertyData)
            {
                return col.ColumnName.ToUpperInvariant() == propertyData.SqlName.ToUpperInvariant();
            }
        }
    }
}
