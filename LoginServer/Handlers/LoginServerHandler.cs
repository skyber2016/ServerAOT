using System.Net.Sockets;
using Core;

namespace LoginServer;

public class LoginServerHandler : AServerHandler
{
    private readonly ILogger _logger = LoggerManager.CreateLogger();
    public LoginServerHandler() : base()
    {
    }

    protected override int Port => ApplicationContext.Instance.AppConfig.LoginServer.LocalPort;

    protected override async Task OnConnected(Socket client, CancellationToken cancellationToken)
    {
        var handler = new LoginClientHandler(client);
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
