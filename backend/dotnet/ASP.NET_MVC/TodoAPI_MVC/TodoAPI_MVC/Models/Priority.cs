using System.Text.Json.Serialization;
using TodoAPI_MVC.Atributtes;
using TodoAPI_MVC.Database.Service;

namespace TodoAPI_MVC
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [DbValueConverter(typeof(DbStringEnumConverter))]
    public enum Priority
    {
        A,
        B,
        C
    }
}