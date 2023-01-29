namespace TodoAPI_MVC.Database.Service
{
    public abstract class DbNamingPolicy
    {
        protected abstract Func<string, string> NameConverter { get; }

        public virtual string ConvertName(string name) => NameConverter(name);
    }
}