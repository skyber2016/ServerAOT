using System.Net.Sockets;
using Core;
using GameServer.Packets;
using static Core.Logging;

namespace GameServer;

internal class GameServerHandler : AProxyHandler
{
    private readonly WriteDelegate _logger = Write(typeof(GameServerHandler));
    protected override int Port { get; }

    protected override string IpProxy { get; }

    public GameServerHandler(Socket clientSocket) : base(clientSocket)
    {
        Port = 9958;
        IpProxy = "127.0.0.1";
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
            handler = fnHandler(_clientSocket, _proxySocket, channel);
        }
        else
        {
            handler = new ForwardPacket(_clientSocket, _proxySocket, channel);
        }
        handler.Load(buffer);
        await handler.HandleAsync(cancellationToken);
    }
}
