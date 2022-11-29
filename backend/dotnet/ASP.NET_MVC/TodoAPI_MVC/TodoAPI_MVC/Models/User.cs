using System.Text.Json.Serialization;

namespace TodoAPI_MVC.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;

        [JsonIgnore]
        public string NormalizedUsername { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;

        [JsonIgnore]
        public string Password { get; set; } = string.Empty;
    }
}
