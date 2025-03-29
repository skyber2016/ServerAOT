using System.Text.Json.Serialization;

namespace Core
{
    [JsonSourceGenerationOptions]
    [JsonSerializable(typeof(AppConfig))]
    [JsonSerializable(typeof(GameSeverConfig))]
    [JsonSerializable(typeof(LoginServerConfig))]
    [JsonSerializable(typeof(ServerConfig))]
    [JsonSerializable(typeof(RedisConfig))]
    [JsonSerializable(typeof(DatabaseConfig))]
    [JsonSerializable(typeof(NpcEntity))]
    [JsonSerializable(typeof(NpcEntity[]))]
    [JsonSerializable(typeof(NpcDelayEntity[]))]
    public partial class ApplicationJsonContext : JsonSerializerContext
    {
    }
}
