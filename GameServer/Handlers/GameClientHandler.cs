using Core;
using System.Net.Sockets;

namespace GameServer;

public class GameClientHandler : AClientHandler
{
    private readonly ILogger _logger = LoggerManager.CreateLogger();
    protected override int ServerPort { get; }

    protected override string ServerIP { get; }

    private readonly UserContext _userContext;

    public GameClientHandler(Socket clientSocket) : base(clientSocket)
    {
        _userContext = new UserContext();
        ServerPort = ApplicationContext.Instance.AppConfig.GameServer.ServerPort;
        ServerIP = ApplicationContext.Instance.AppConfig.GameServer.ServerIP;
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
        try
        {
            handler.Load(buffer);
            await handler.HandleAsync(_userContext, cancellationToken);
        }
        finally
        {
            handler.Dispose();
        }
    }
}
