using System.Net.Sockets;
using Core;

namespace GameServer
{
    public class RobotResponsePacket : APacketHandler
    {
        private int _padding { get; set; } = 0;
        private readonly ILogger _logger = LoggerManager.CreateLogger();
        public RobotResponsePacket(Socket client, Socket proxy, Channel channel) : base(client, proxy, channel)
        {
        }

        protected override async Task PacketHandleAsync(UserContext context, CancellationToken cancellationToken)
        {
            _logger.Info($"{typeof(RobotResponsePacket)} executing...");
            var buffers = _memoryStream.ToArray();
            this.Load(context);
            await _client.SendAsync(buffers);
        }

        private void Load(UserContext context)
        {
            var robot = new RobotModel();
            _padding += 8;
            _reader.ReadBytes(_padding);
            robot.Id = _reader.ReadUInt32();
            robot.Type = _reader.ReadUInt32();
            context.Robot = robot;
        }
    }
}
