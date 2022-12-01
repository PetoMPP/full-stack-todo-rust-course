namespace TodoAPI_MVC.Database
{
    internal class DbHelpers
    {
        internal static string GetSqlValue(object? value)
        {
            if (value is null)
                return "null";
            if (value is string or char or Enum)
                return $"'{value}'";
            if (value is DateTime dateTime)
                return $"'{dateTime.ToString(Consts.DateFormat)}'";

            return value.ToString() ?? string.Empty;
        }
    }
}
