using Core;
using System.Net.Sockets;

namespace GameServer
{
    public class UserInfoResponsePacket : APacketHandler
    {
        private readonly ILogger _logger = LoggerManager.CreateLogger();
        public UserInfoResponsePacket(Socket client, Socket proxy, Channel channel) : base(client, proxy, channel)
        {
        }

        protected override async Task PacketHandleAsync(UserContext context, CancellationToken cancellationToken)
        {
            _logger.Info($"{typeof(UserInfoResponsePacket)} executing...");
            var buffers = _memoryStream.ToArray();
            this.Load(context);
            await _proxy.SendAsync(buffers);
        }

        private void Load(UserContext context)
        {
            _reader.ReadUInt32();
            context.UserId = _reader.ReadUInt32();
        }
    }
}
