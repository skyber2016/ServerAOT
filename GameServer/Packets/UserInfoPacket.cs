using Core;
using System.Net.Sockets;

namespace GameServer
{
    public class UserInfoPacket : APacketHandler
    {
        private readonly ILogger _logger = LoggerManager.CreateLogger();
        public UserInfoPacket(Socket client, Socket proxy, Channel channel) : base(client, proxy, channel)
        {
        }

        protected override async Task PacketHandleAsync(UserContext context, CancellationToken cancellationToken)
        {
            var buffers = _memoryStream.ToArray();
            if(_channel == Channel.C2S)
            {
                await _proxy.SendAsync(buffers);
                return;
            }
            var hex = buffers.Select(x => x.ToString("X"));
            _logger.Info($"{typeof(UserInfoPacket)} {string.Join(' ', hex)}");
            await _proxy.SendAsync(buffers);
        }

    }
}
