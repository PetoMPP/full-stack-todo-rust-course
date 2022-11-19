using System.Text.Json.Serialization;

namespace TodoAPI_MVC.Models
{
    public record struct ApiResponse(
        StatusCode Code,
        [property:JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        object? Data = null,
        [property:JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        string? Error = null);
}
