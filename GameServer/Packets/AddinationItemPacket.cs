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

        public AddinationItemPacket(Socket client, Socket proxy, Channel channel, IServiceProvider serviceProvider) : base(client, proxy, channel)
        {
            _databaseService = serviceProvider.GetRequiredService<IDatabaseService>();
        }

        protected override async Task PacketHandleAsync(UserContext context, CancellationToken cancellationToken)
        {
            var buffers = _memoryStream.ToArray();
			_logger.Debug($"Addination item [acc_id={context.AccountId}]: {string.Join(' ', buffers.Select(x=> x.ToString("X")))}");
            this.Load();
            var itemLen = await this.CheckItemExisted([FirstItem, SecondItem]);
            if (itemLen != 2)
            {
                _logger.Critical($"acc_id={context.AccountId} char_name={context.CharName} usage second itemex_id dose not existed.");
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

        private async Task<int> CheckItemExisted(uint[] lstId)
        {
            var json = await _databaseService.ExecuteAsync(new QueryNative
            {
                Sql = string.Format(SqlNative.SQL_GET_ITEMEX_BY_ID, string.Join(',', lstId)),
                Payload = []
            });
            var items = JsonSerializer.Deserialize(json, ApplicationJsonContext.Default.ItemexEntityArray);
            return items.Length;
        }
    }
}
