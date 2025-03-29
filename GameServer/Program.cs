using Core;
using GameServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using IHost host = Host.CreateDefaultBuilder()
        .ConfigureServices((_, services) =>
        {
            services.AddSingleton<AServerHandler, GameServerHandler>();
            services.AddSingleton<IRedisService, RedisService>();
            services.AddSingleton<IDatabaseService, DatabaseService>();
        })
        .Build();
var _serviceProvider = host.Services;

var _logger = LoggerManager.CreateLogger();

AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
{
    if (args.ExceptionObject is Exception exception)
        _logger.Critical(exception.GetBaseException().Message);
};

TaskScheduler.UnobservedTaskException += (sender, e) =>
{
    _logger.Critical($"[Unobserved Task] {e.Exception.GetBaseException().Message}");
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


PacketRegistry.TryAdd(PacketType.UserInfoRequest, (c, s, channel) => new UserInfoRequestPacket(c, s, channel));
PacketRegistry.TryAdd(PacketType.RobotRepsonse, (c, s, channel) => new RobotResponsePacket(c, s, channel));
PacketRegistry.TryAdd(PacketType.UserInfoResponse, (c, s, channel) => new UserInfoResponsePacket(c, s, channel));
PacketRegistry.TryAdd(PacketType.ClickNpcRequest, (c, s, channel) => new NpcClickActionPacket(c, s, channel, _serviceProvider));
PacketRegistry.TryAdd(PacketType.NPCAction, (c, s, channel) => new NpcActionPacket(c, s, channel, _serviceProvider));


await Worker(cts.Token);

async Task Worker(CancellationToken ct)
{
    var server = _serviceProvider.GetRequiredService<AServerHandler>();
    server.Init();
    await server.StartAsync(ct);
}

