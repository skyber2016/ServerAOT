using Core;
using System.Net.Sockets;

namespace GameServer
{
    public class NpcClickActionPacket : APacketHandler
    {
        private readonly ILogger _logger = LoggerManager.CreateLogger();
        public NpcClickActionPacket(Socket client, Socket proxy, Channel channel, IServiceProvider serviceProvider) : base(client, proxy, channel)
        {
        }

        protected override async Task PacketHandleAsync(UserContext context, CancellationToken cancellationToken)
        {
            var ctx = ApplicationContext.Instance;
            var caching = ctx.Caching;
            caching.RemoveByPattern($"NPC_DELAY:{context.AccountId}:*");
            var buffers = _memoryStream.ToArray();
            _reader.ReadBytes(4);
            int npcId = _reader.ReadInt32();
            var delays = ctx.NpcDelays.Where(x => x.NpcId == npcId).ToArray();
            foreach (var item in delays)
            {
                if (string.IsNullOrEmpty(item.Options))
                {
                    continue;
                }
                caching.Set($"NPC_DELAY:{context.AccountId}:{item.Options}", item, ApplicationInfo.TimeSpanMax);
            }
            caching.Set($"NPC_DELAY:{context.AccountId}:CURRENT", string.Empty, ApplicationInfo.TimeSpanMax);
            await _proxy.SendAsync(buffers);
        }

    }
}
