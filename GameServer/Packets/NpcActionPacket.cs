using System.Net.Sockets;
using System.Text.Json;
using Core;
using Microsoft.Extensions.DependencyInjection;

namespace GameServer
{
    public class NpcActionPacket : APacketHandler
    {
        private readonly ILogger _logger = LoggerManager.CreateLogger();
        private readonly IDatabaseService _databaseService;
        private readonly IRedisService _redisService;
        private static SemaphoreSlim _signal = new(1, 1);
        public NpcActionPacket(Socket client, Socket proxy, Channel channel, IServiceProvider serviceProvider) : base(client, proxy, channel)
        {
            _databaseService = serviceProvider.GetRequiredService<IDatabaseService>();
            _redisService = serviceProvider.GetRequiredService<IRedisService>();
        }

        protected override async Task PacketHandleAsync(UserContext context, CancellationToken cancellationToken)
        {
            var buffers = _memoryStream.ToArray();
            var paddings = _reader.ReadBytes(9);
            var choise = _reader.ReadByte();

            if (_channel == Channel.S2C)
            {
                await _client.SendAsync(buffers, cancellationToken);
                return;
            }

            var key = $"NPC_DELAY:{context.AccountId}";
            var optionChoise = await _redisService.GetAsync($"{key}:CURRENT");
            if (string.IsNullOrEmpty(optionChoise))
            {
                optionChoise = choise.ToString();
            }
            else
            {
                optionChoise = $"{optionChoise}:{choise}";
            }
            await _redisService.SetAsync($"{key}:CURRENT", optionChoise);
            var delay = await _redisService.GetAsync($"{key}:{optionChoise}");
            if (string.IsNullOrEmpty(delay))
            {
                await _proxy.SendAsync(buffers);
                return;
            }

            var npcDelay = JsonSerializer.Deserialize(delay, ApplicationJsonContext.Default.NpcDelayEntity);
            
            var newKey = $"NPC_DELAY:COOLDOWN:{npcDelay.NpcId}";
            if (await _redisService.KeyExistedAsync(newKey))
            {
                _logger.Info($"user_id={context.AccountId} npc_id={npcDelay.NpcId} is delaying");
                await _client.SystemNotification($"NPC is avaiable after {npcDelay.DelayTime}");
                return;
            }
            await _redisService.SetAsync(newKey, delay, TimeSpan.FromMilliseconds(npcDelay.DelayTime));
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

                if (!await _redisService.SetAsync(RedisKeyConstants.NPC_DEPLAY_KEY, npcDelaysJson, TimeSpan.FromHours(1)))
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
