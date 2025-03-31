using Core;
using GameServer;
using GameServer.Packets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

using IHost host = Host.CreateDefaultBuilder()
        .ConfigureServices((_, services) =>
        {
            services.AddSingleton<AServerHandler, GameServerHandler>();
            services.AddSingleton<IRedisService, RedisService>();
            services.AddSingleton<IDatabaseService, DatabaseService>();
            services.AddSingleton<INotificationService, NotificationService>();
        })
        .Build();
var _serviceProvider = host.Services;

var _logger = LoggerManager.CreateLogger();

void WriteLog(string message, Exception ex)
{
    if (ex.InnerException != null)
    {
        WriteLog(message, ex.InnerException);
    }
    _logger.Critical($"{message} {ex.Message}");
    _logger.Critical($"{message} {ex.StackTrace}");
}

AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
{
    if (args.ExceptionObject is Exception exception)
    {
        WriteLog("[Unhandled] ", exception);
    }
};

TaskScheduler.UnobservedTaskException += (sender, e) =>
{
    WriteLog("[Unobserved Task] ", e.Exception);
    e.SetObserved(); // Đánh dấu lỗi đã được xử lý để tránh crash
};

CancellationTokenSource cts = new CancellationTokenSource();
Console.WriteLine("Application has started. Ctrl-C to end");

Console.CancelKeyPress += (sender, eventArgs) =>
{
    Console.WriteLine("Cancel event triggered");
    cts.Cancel();
    eventArgs.Cancel = true;
};


PacketRegistry.TryAdd(PacketType.UserInfoRequest, (c, s, channel) => new UserInfoRequestPacket(c, s, channel, _serviceProvider));
//PacketRegistry.TryAdd(PacketType.RobotRepsonse, (c, s, channel) => new RobotResponsePacket(c, s, channel));
PacketRegistry.TryAdd(PacketType.ClickNpcRequest, (c, s, channel) => new NpcClickActionPacket(c, s, channel, _serviceProvider));
PacketRegistry.TryAdd(PacketType.NPCAction, (c, s, channel) => new NpcActionPacket(c, s, channel, _serviceProvider));
PacketRegistry.TryAdd(PacketType.AddinationItemRequest, (c, s, channel) => new AddinationItemPacket(c, s, channel, _serviceProvider));


await Worker(cts.Token);

async Task Worker(CancellationToken ct)
{
    _logger.Debug("Loading cq_npc_delay...");
    ApplicationContext.Instance.NpcDelays = await GetDelays(ct);
    _logger.Debug($"Loaded {ApplicationContext.Instance.NpcDelays.Length} cq_npc_delay");
    var server = _serviceProvider.GetRequiredService<AServerHandler>();
    server.Init();
    await server.StartAsync(ct);
}


async Task<NpcDelayEntity[]> GetDelays(CancellationToken cancellationToken)
{
    try
    {
        var _databaseService = _serviceProvider.GetRequiredService<IDatabaseService>();
        var npcDelaysJson = await _databaseService.ExecuteAsync(new QueryNative
        {
            Sql = SqlNative.SQL_NPC_DELAY,
            Payload = []
        });
        return JsonSerializer.Deserialize(npcDelaysJson, ApplicationJsonContext.Default.NpcDelayEntityArray);
    }
    catch (Exception ex)
    {
        _logger.Error($"Get npc delay failure. {ex.GetBaseException().Message}");
        return null;
    }
}