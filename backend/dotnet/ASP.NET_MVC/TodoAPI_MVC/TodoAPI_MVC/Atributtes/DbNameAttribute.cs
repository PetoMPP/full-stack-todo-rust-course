namespace TodoAPI_MVC.Atributtes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DbNameAttribute : Attribute
    {
        public string ColumnName { get; set; }

        public DbNameAttribute(string columnName)
        {
            ColumnName = columnName;
        }
    }
}
