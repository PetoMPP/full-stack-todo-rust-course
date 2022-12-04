namespace TodoAPI_MVC.Database.Service
{
    public class DbStringEnumConverter : DbValueConverter
    {
        public override string Convert(object? value)
        {
            return $"'{value}'";
        }
    }
}
