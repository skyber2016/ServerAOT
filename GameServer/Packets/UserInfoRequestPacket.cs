using System.Net.Sockets;
using Core;

namespace GameServer
{
    public class UserInfoRequestPacket : APacketHandler
    {
        private readonly ILogger _logger = LoggerManager.CreateLogger();
        public UserInfoRequestPacket(Socket client, Socket proxy, Channel channel) : base(client, proxy, channel)
        {
        }

        protected override async Task PacketHandleAsync(UserContext context, CancellationToken cancellationToken)
        {
            _logger.Info($"{typeof(UserInfoRequestPacket)} executing...");
            var buffers = _memoryStream.ToArray();
            this.Load(context);
            await _proxy.SendAsync(buffers);
        }

        private void Load(UserContext context)
        {
            _reader.ReadUInt16();
            _reader.ReadUInt16();
            context.AccountId = _reader.ReadUInt32();
            context.TokenId = _reader.ReadUInt32();
        }
    }
}
