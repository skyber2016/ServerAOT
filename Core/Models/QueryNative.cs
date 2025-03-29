using System.Text.Json.Serialization;

namespace Core
{
    public class QueryNative
    {
        [JsonPropertyName("sql")]
        public string Sql { get; set; }

        [JsonPropertyName("payload")]
        public object[] Payload { get; set; }
    }
}
