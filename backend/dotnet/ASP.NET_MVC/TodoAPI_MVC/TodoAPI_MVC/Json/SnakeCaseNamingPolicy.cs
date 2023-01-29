using System.Text.Json;

namespace TodoAPI_MVC.Json
{
    public class SnakeCaseNamingPolicy : JsonNamingPolicy
    {
        public static SnakeCaseNamingPolicy SnakeCase { get; } = new();
        public override string ConvertName(string name)
        {
            return string.Concat(
                name.Select((c, i) =>
                    char.IsUpper(c)
                        ? $"{(i > 0 ? "_" : "")}{c}".ToLowerInvariant()
                        : $"{c}"));
        }
    }
}
