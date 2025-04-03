using Core;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Sockets;
using System.Text.Json;
using System.Linq;

namespace GameServer.Packets
{
    public class AddinationItemPacket : APacketHandler
    {
        private uint FirstItem { get; set; }
        private uint SecondItem { get; set; }

        private readonly ILogger _logger = LoggerManager.CreateLogger();
        private readonly IDatabaseService _databaseService;
        private readonly IMemoryCacheService _memoryCache;

        public AddinationItemPacket(Socket client, Socket proxy, Channel channel, IServiceProvider serviceProvider) : base(client, proxy, channel)
        {
            _databaseService = serviceProvider.GetRequiredService<IDatabaseService>();
            _memoryCache = ApplicationContext.Instance.Caching;
        }

        protected override async Task PacketHandleAsync(UserContext context, CancellationToken cancellationToken)
        {
            var buffers = _memoryStream.ToArray();
            if(_channel == Channel.S2C)
            {
                _logger.Info($"Skipped to check item exist");
                await _client.SendAsync(buffers);
                return;
            }
			_logger.Debug($"Addination item [acc_id={context.AccountId}]: {string.Join(' ', buffers.Select(x=> x.ToString("X")))}");
            this.Load();
            
            var itemLen = await this.CountItemex(this.FirstItem, this.SecondItem);
            if (itemLen != 2)
            {
                _logger.Critical($"acc_id={context.AccountId} char_name={context.CharName} usage item_id=[{FirstItem} {SecondItem}] dose not existed.");
                return;
            }
            await _proxy.SendAsync(buffers, cancellationToken);
			_logger.Info($"acc_id={context.AccountId} char_name={context.CharName} usage first_item_id={FirstItem} & second_item_id={SecondItem}");
            return;
        }

        private void Load()
        {
            _reader.ReadUInt32();
            this.FirstItem = _reader.ReadUInt32();
            this.SecondItem = _reader.ReadUInt32();
        }
        private async Task DeleteItem(uint itemId)
        {
            var json = await _databaseService.ExecuteAsync(new QueryNative
            {
                Sql = string.Format(SqlNative.SQL_DELETE_ITEMEX_BY_ID, itemId),
                Payload = []
            });
            _logger.Info($"Deleted item_id={itemId} result={json}");
        }
        private async Task<int> CountItemex(uint first, uint second)
        {
            var json = await _databaseService.ExecuteAsync(new QueryNative
            {
                Sql = string.Format(SqlNative.SQL_GET_ITEMEX_BY_ID, first, second),
                Payload = []
            });
            var items = JsonSerializer.Deserialize(json, ApplicationJsonContext.Default.ItemexEntityArray);
            return items.Length;
        }
    }
}
