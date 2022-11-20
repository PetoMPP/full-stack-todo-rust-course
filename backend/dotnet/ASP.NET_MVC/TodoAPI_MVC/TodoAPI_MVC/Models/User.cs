using System.Text.Json.Serialization;

namespace TodoAPI_MVC.Models
{
    public class User
    {
        private string _username = string.Empty;
        private string _normalizedUsername = string.Empty;

        public int Id { get; set; }
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                _normalizedUsername = value.ToUpper();
            }
        }

        [JsonIgnore]
        public string NormalizedUsername => _normalizedUsername;
        public string Token { get; set; } = string.Empty;

        public User()
        {
        }
    }
}
