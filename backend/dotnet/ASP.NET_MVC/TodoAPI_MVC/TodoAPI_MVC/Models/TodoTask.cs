using System.Text.Json.Serialization;
using TodoAPI_MVC.Atributtes;
using TodoAPI_MVC.Json;

namespace TodoAPI_MVC.Models
{
    public record struct TodoTask(
        [property: DbDefault]
        int Id,
        string Title,
        Priority? Priority,
        string? Description,
        [property: JsonConverter(typeof(DateTimeJsonConverterFactory))]
        [property: DbDefault]
        DateTime CreatedAt,
        [property: JsonConverter(typeof(DateTimeJsonConverterFactory))]
        DateTime? CompletedAt,
        int UserId)
    {
        public string? Validate()
        {
            if (string.IsNullOrWhiteSpace(Title))
                return $"{nameof(Title)} cannot be empty!";

            return null;
        }
    }
}