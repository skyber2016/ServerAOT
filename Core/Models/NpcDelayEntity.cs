using System.Text.Json.Serialization;

namespace Core;

public class NpcDelayEntity
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("npc_id")]
    public int NpcId { get; set; }

    [JsonPropertyName("options")]
    public string Options { get; set; }

    [JsonPropertyName("delay_time")]
    public uint DelayTime { get; set; }

    public TimeSpan GetDelay() => TimeSpan.FromMilliseconds(DelayTime);

    public List<int> GetOptions()
    {
        if (string.IsNullOrEmpty(this.Options))
        {
            return [];
        }
        return this.Options.Split(',').Select(x => int.Parse(x)).ToList();
    }

    public void SetOption(int option)
    {
        var options = this.GetOptions();
        options.Add(option);
        this.Options = string.Join(",", options);
    }

}
