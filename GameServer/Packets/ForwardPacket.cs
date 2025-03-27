using System.Net.Sockets;
using Core;

namespace GameServer.Packets
{
    public class ForwardPacket : APacketHandler
    {
        public ForwardPacket(Socket client, Socket proxy, Channel channel) : base(client, proxy, channel)
        {
        }

        protected override async Task PacketHandleAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            byte[] buffer = _memoryStream.ToArray();
            if (_channel == Channel.C2S)
            {
                await _proxy.SendAsync(buffer, cancellationToken);
            }
            else
            {
                await _client.SendAsync(buffer, cancellationToken);
            }
        }
    }
}
