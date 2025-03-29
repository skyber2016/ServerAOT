using System.Text.Json.Serialization;

namespace Core
{
    public class NpcEntity
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("task0")]
        public uint ActionId { get; set; }
    }
}
