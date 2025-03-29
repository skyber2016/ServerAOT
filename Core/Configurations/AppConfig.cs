namespace Core;

public class AppConfig
{
    public LoginServerConfig LoginServer { get; set; }
    public GameSeverConfig GameServer { get; set; }
    public RedisConfig RedisConfig { get; set; }
    public DatabaseConfig DatabaseConfig { get; set; }
}

