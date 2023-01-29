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
        DateTime? CreatedAt,
        [property: JsonConverter(typeof(DateTimeJsonConverterFactory))]
        DateTime? CompletedAt,
        int UserId)
    {
        private const int MinTitleLenght = 3;

        public string? Validate()
        {
            if (Title.Length < MinTitleLenght)
                return $"The {nameof(Title)} must be at least {MinTitleLenght} characters long!";

            return null;
        }
    }
}