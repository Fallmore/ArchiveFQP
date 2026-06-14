using ArchiveFqp.Models.DTO.User;
using System.Text.Json.Serialization;

namespace ArchiveFqp.Models.Auth.ThreeKL
{
    public class ThreeKlTokens
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;
    }

    public class ThreeKlUserInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("firstname")]
        public string Firstname { get; set; } = string.Empty;

        [JsonPropertyName("lastname")]
        public string Lastname { get; set; } = string.Empty;

        [JsonIgnore]
        public RegisterModel RegisterModel { get; set; } = new()
        { 
            Surname = "Н/Д",
            Name = "Н/Д",
            Login = "Н/Д",
            Password = "Пароль1234!",
            ConfirmPassword = "Пароль1234!"
        };
    }   
}
