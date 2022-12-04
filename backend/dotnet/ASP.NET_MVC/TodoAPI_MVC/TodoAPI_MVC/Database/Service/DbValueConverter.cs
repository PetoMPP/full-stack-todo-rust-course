namespace TodoAPI_MVC.Database.Service
{
    public abstract class DbValueConverter
    {
        public abstract string Convert(object? value);
    }
}
