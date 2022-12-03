namespace TodoAPI_MVC.Database.Service
{
    public class SnakeCaseNamingPolicy : DbNamingPolicy
    {
        protected override Func<string, string> NameConverter =>
            (name) => string.Concat(name.Select((c, i) => char.IsUpper(c)
                ? $"{(i > 0 ? "_" : "")}{c}".ToLowerInvariant()
                : $"{c}"));
    }
}
