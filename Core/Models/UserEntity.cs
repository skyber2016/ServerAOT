using System.Text.Json.Serialization;

namespace Core
{
    public class UserEntity
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("account_id")]
        public uint AccountId { get; set; }
    }
}
