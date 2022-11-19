using System.Text.Json.Serialization;
using TodoAPI_MVC.Json;

namespace TodoAPI_MVC.Models
{
    public record struct TodoTask(
        int Id,
        string Title,
        Priority? Priority,
        string? Description,
        [property: JsonConverter(typeof(DateTimeJsonConverter))]
        [property: JsonPropertyName("completed_at")]
        DateTime? CompletedAt)
    {
        public string? Validate()
        {
            if (string.IsNullOrWhiteSpace(Title))
                return $"{nameof(Title)} cannot be empty!";

            return null;
        }
    }
}