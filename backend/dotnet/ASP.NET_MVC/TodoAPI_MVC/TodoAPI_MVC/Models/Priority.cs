using System.Text.Json.Serialization;

namespace TodoAPI_MVC
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Priority
    {
        A,
        B,
        C
    }
}