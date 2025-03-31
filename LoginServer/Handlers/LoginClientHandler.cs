using Core;
using System.Net.Sockets;

namespace LoginServer;

public class LoginClientHandler : AClientHandler
{
    private readonly ILogger _logger = LoggerManager.CreateLogger();
    protected override int ServerPort { get; }

    protected override string ServerIP { get; }

    public LoginClientHandler(Socket clientSocket) : base(clientSocket)
    {
        ServerPort = ApplicationContext.Instance.AppConfig.LoginServer.ServerPort;
        ServerIP = ApplicationContext.Instance.AppConfig.LoginServer.ServerIP;
    }

    protected override async Task OnReceived(byte[] buffer, Channel channel, CancellationToken cancellationToken)
    {
        using var memoryStream = new MemoryStream(buffer);
        using var reader = new BinaryReader(memoryStream);
        var packetLength = reader.ReadInt16();
        var packetType = (PacketType)reader.ReadInt16();
        APacketHandler handler = null;

        if (PacketRegistry.TryGetValue(packetType, out var fnHandler))
        {
            handler = fnHandler(_clientSocket, _serverSocket, channel);
        }
        else
        {
            handler = new ForwardPacket(_clientSocket, _serverSocket, channel);
        }
        handler.Load(buffer);
        await handler.HandleAsync(null, cancellationToken);
        handler.Dispose();
    }
}
