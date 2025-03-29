using Core;
using LoginServer;

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

PacketRegistry.TryAdd(PacketType.LoginRequest, (client, proxy, channel) => new LoginRequestPacket(client, proxy, channel));
PacketRegistry.TryAdd(PacketType.LoginResponse, (client, proxy, channel) => new LoginResponsePacket(client, proxy, channel));


CancellationTokenSource cts = new CancellationTokenSource();
Console.WriteLine("Application has started. Ctrl-C to end");

Console.CancelKeyPress += (sender, eventArgs) =>
{
    Console.WriteLine("Cancel event triggered");
    cts.Cancel();
    eventArgs.Cancel = true;
};

await Worker(cts.Token);

async Task Worker(CancellationToken ct)
{
    var loginServer = new LoginServerHandler();
    loginServer.Init();
    await loginServer.StartAsync(ct);
}


