using Core;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Sockets;

namespace GameServer
{
    public class NpcActionPacket : APacketHandler
    {
        private readonly ILogger _logger = LoggerManager.CreateLogger();
        private readonly INotificationService _notiService;
        public NpcActionPacket(Socket client, Socket proxy, Channel channel, IServiceProvider serviceProvider) : base(client, proxy, channel)
        {
            _notiService = serviceProvider.GetRequiredService<INotificationService>();
        }

        protected override async Task PacketHandleAsync(UserContext context, CancellationToken cancellationToken)
        {
            var buffers = _memoryStream.ToArray();
            if (_channel == Channel.S2C)
            {
                await _client.SendAsync(buffers, cancellationToken);
                return;
            }

            var paddings = _reader.ReadBytes(9);
            var choise = _reader.ReadByte();
            var caching = ApplicationContext.Instance.Caching;

            var key = $"NPC_DELAY:{context.AccountId}";
            if (!caching.TryGetValue($"{key}:CURRENT", out string optionChoise))
            {
                optionChoise = choise.ToString();
            }
            else
            {
                if (string.IsNullOrEmpty(optionChoise))
                {
                    optionChoise = choise.ToString();
                }
                else
                {
                    optionChoise = $"{optionChoise}:{choise}";
                }
            }
            caching.Set($"{key}:CURRENT", optionChoise, ApplicationInfo.TimeSpanMax);
            if (!caching.TryGetValue($"{key}:{optionChoise}", out NpcDelayEntity npcDelay))
            {
                await _proxy.SendAsync(buffers);
                return;
            }
            var newKey = $"NPC_DELAY:COOLDOWN:{npcDelay.NpcId}";
            if (caching.TryGetValue(newKey, out DateTime exp))
            {
                TimeSpan remaining = exp - DateTime.Now;
                if (remaining > TimeSpan.Zero)
                {
                    string formattedTime = $"{remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
                    string msg = $"[{context.AccountId} {context.CharName}] must wait {formattedTime} before performing this action again.";
                    _logger.Info(msg);
                    //await _notiService.SendNotification(_client, NotificationType.System, context.AccountId, msg, cancellationToken);
                    return;
                }
            }
            caching.Set(newKey, DateTime.Now.AddMilliseconds(npcDelay.DelayTime), TimeSpan.FromMilliseconds(npcDelay.DelayTime));
            await _proxy.SendAsync(buffers);
        }



    }
}
