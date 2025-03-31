using Core;
using System.Net.Sockets;

namespace GameServer;

public class GameServerHandler : AServerHandler
{
    protected override int Port => ApplicationContext.Instance.AppConfig.GameServer.LocalPort;

    private readonly ILogger _logger = LoggerManager.CreateLogger();

    public GameServerHandler() : base()
    {
    }

    protected override async Task OnConnected(Socket client, CancellationToken cancellationToken)
    {
        var handler = new GameClientHandler(client);
        if (!await handler.InitAsync(cancellationToken))
        {
            _logger.Error("Failed to init proxy");
            client.Dispose();
            return;
        }
        ClientRegistry.TryAdd(client, handler);
        _ = Task.Run(async () =>
        {
            await handler.HandleAsync(cancellationToken);
            ClientRegistry.Remove(client);
            Console.WriteLine($"Client {client.RemoteEndPoint} has disconnected");
        }, cancellationToken);
    }

}
