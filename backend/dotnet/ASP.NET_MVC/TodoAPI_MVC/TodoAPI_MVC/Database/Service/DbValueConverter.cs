namespace TodoAPI_MVC.Database.Service
{
    public abstract class DbValueConverter
    {
        public abstract Func<object?, string> Convert { get; }
        public abstract Func<Type, bool> CanConvert { get; }
    }
}
