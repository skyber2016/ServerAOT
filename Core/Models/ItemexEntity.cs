using System.Text.Json.Serialization;

namespace Core;

public class ItemexEntity
{
    [JsonPropertyName("id")]
    public uint Id { get; set; }

    [JsonPropertyName("type")]
    public uint Type { get; set; }
}
