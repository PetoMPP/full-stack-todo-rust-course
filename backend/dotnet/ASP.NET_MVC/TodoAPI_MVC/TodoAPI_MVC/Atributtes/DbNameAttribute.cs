namespace TodoAPI_MVC.Atributtes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DbNameAttribute : Attribute
    {
        public string ColumnName { get; }

        public DbNameAttribute(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                throw new ArgumentException(
                    $"{nameof(columnName)} cannot be empty!", nameof(columnName));

            ColumnName = columnName;
        }
    }
}
