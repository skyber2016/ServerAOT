using System.Text.Json.Serialization;

namespace Core
{
    [JsonSourceGenerationOptions]
    [JsonSerializable(typeof(QueryNative))]
    public partial class QueryNativeContext : JsonSerializerContext
    {
    }
}
