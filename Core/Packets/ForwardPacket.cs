using System.Net.Sockets;

namespace Core
{
    public class ForwardPacket : APacketHandler
    {
        public ForwardPacket(Socket client, Socket proxy, Channel channel) : base(client, proxy, channel)
        {
        }

        protected override async Task PacketHandleAsync(UserContext context, CancellationToken cancellationToken)
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
