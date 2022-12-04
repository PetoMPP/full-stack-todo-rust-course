using System.Text.Json.Serialization;
using TodoAPI_MVC.Atributtes;

namespace TodoAPI_MVC.Models
{
    public class User
    {
        [DbDefault]
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        [JsonIgnore]
        public string NormalizedUsername { get; set; } = string.Empty;

        [DbIgnore]
        public string Token { get; set; } = string.Empty;

        [JsonIgnore]
        public string Password { get; set; } = string.Empty;

        public EndpointAccess Access { get; set; } = EndpointAccess.TasksOwned;
    }
}
