using System.Net.Sockets;
using Core;
using Core.Providers;

namespace GameServer;

public class GameServer : AServerHandler
{
    protected override int Port => 9958;

    protected override async Task OnConnected(Socket client, CancellationToken cancellationToken)
    {
        var handler = new GameServerHandler(client);
        if(!await handler.InitAsync(cancellationToken))
        {
            _logger("Failed to init proxy");
            client.Dispose();
            return;
        }
        ProxyRegistry.TryAdd(client, handler);
        _ = Task.Run(async () =>
        {
            await handler.HandleAsync(cancellationToken);
            ProxyRegistry.Remove(client);
            Console.WriteLine($"Client {client.RemoteEndPoint} has disconnected");
        }, cancellationToken);
    }

}
