using System.Net.Sockets;
using System.Text.Json;
using Core;
using Microsoft.Extensions.DependencyInjection;

namespace GameServer
{
    public class NpcClickActionPacket : APacketHandler
    {
        private readonly ILogger _logger = LoggerManager.CreateLogger();
        private readonly IDatabaseService _databaseService;
        private readonly IRedisService _redisService;
        private static SemaphoreSlim _signal = new(1, 1);
        public NpcClickActionPacket(Socket client, Socket proxy, Channel channel, IServiceProvider serviceProvider) : base(client, proxy, channel)
        {
            _databaseService = serviceProvider.GetRequiredService<IDatabaseService>();
            _redisService = serviceProvider.GetRequiredService<IRedisService>();
        }

        protected override async Task PacketHandleAsync(UserContext context, CancellationToken cancellationToken)
        {
            await _redisService.DeleteKeysByPatternAsync($"NPC_DELAY:{context.AccountId}:*");
            var buffers = _memoryStream.ToArray();
            _reader.ReadBytes(4);
            int npcId = _reader.ReadInt32();
            var delays = await this.GetDelays(cancellationToken);
            var entities = delays.Where(x => x.NpcId == npcId).ToArray();
            foreach (var item in entities)
            {
                if (string.IsNullOrEmpty(item.Options))
                {
                    continue;
                }
                string opt = string.Join(':', item.Options.Split(','));
                var json = JsonSerializer.Serialize(item, ApplicationJsonContext.Default.NpcDelayEntity);
                await _redisService.SetAsync($"NPC_DELAY:{context.AccountId}:{opt}", json);
            }

            await _redisService.SetAsync($"NPC_DELAY:{context.AccountId}:CURRENT", string.Empty);
            await _proxy.SendAsync(buffers);
        }

        private async Task<NpcDelayEntity[]> GetDelays(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            try
            {
                var npcDelaysJson = await _redisService.GetAsync(RedisKeyConstants.NPC_DEPLAY_KEY);
                if (string.IsNullOrEmpty(npcDelaysJson))
                {
                    npcDelaysJson = await _databaseService.ExecuteAsync(new QueryNative
                    {
                        Sql = SqlNative.SQL_NPC_DELAY,
                        Payload = []
                    });
                }
                if (npcDelaysJson == null)
                {
                    return null;
                }

                if (!await _redisService.SetAsync(RedisKeyConstants.NPC_DEPLAY_KEY, npcDelaysJson, TimeSpan.FromHours(24)))
                {
                    _logger.Error($"Cannot set npc delay to redis");
                }
                return JsonSerializer.Deserialize(npcDelaysJson, ApplicationJsonContext.Default.NpcDelayEntityArray);
            }
            catch (Exception ex)
            {
                _logger.Error($"Get npc delay failure. {ex.GetBaseException().Message}");
                return null;
            }
            finally
            {
                _signal.Release();
            }
        }
    }
}
