using System.Text.Json;

namespace Core;

public class ApplicationContext
{
    private static readonly Lazy<ApplicationContext> _instance = new(() => new ApplicationContext());
    public static ApplicationContext Instance => _instance.Value;

    public AppConfig AppConfig { get; private set; }

    public NpcEntity[] Npc { get; set; }

    public ApplicationContext()
    {
        this.AppConfig = this.LoadConfig("appsettings.json");
    }


    private AppConfig LoadConfig(string configPath)
    {
        var pathToFile = Path.Combine(ApplicationInfo.AppBaseDirectory, configPath);
        if (!File.Exists(pathToFile))
        {
            throw new FileNotFoundException($"Could not found file in {pathToFile}");
        }

        string json = File.ReadAllText(pathToFile);
        return JsonSerializer.Deserialize(json, ApplicationJsonContext.Default.AppConfig)
               ?? throw new InvalidOperationException("Error when load config");
    }
}
