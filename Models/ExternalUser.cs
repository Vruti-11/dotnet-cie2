using System.Text.Json.Serialization;

namespace dotnet_cie2.Models
{
    public class ExternalUser
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("phone")]
        public string Phone { get; set; } = string.Empty;

        [JsonPropertyName("website")]
        public string Website { get; set; } = string.Empty;

        [JsonPropertyName("company")]
        public CompanyInfo? Company { get; set; }
    }

    public class CompanyInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("catchPhrase")]
        public string CatchPhrase { get; set; } = string.Empty;

        [JsonPropertyName("bs")]
        public string Bs { get; set; } = string.Empty;
    }
}
