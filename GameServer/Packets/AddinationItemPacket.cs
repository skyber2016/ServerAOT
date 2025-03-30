using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Core;
using Microsoft.Extensions.DependencyInjection;

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
            this.Load();
            var secondItemIsExisted = await this.CheckItemExisted(this.SecondItem);
            if (!secondItemIsExisted)
            {
                _logger.Critical($"acc_id={context.AccountId} usage second itemex_id dose not existed.");
                return;
            }
            var firstItemIsExisted = await this.CheckItemExisted(this.FirstItem);
            if (!firstItemIsExisted)
            {
                _logger.Critical($"acc_id={context.AccountId} usage first itemex_id dose not existed.");
                return;
            }
            await _proxy.SendAsync(buffers, cancellationToken);
            return;
        }

        private void Load()
        {
            _reader.ReadUInt32();
            this.FirstItem = _reader.ReadUInt32();
            this.SecondItem = _reader.ReadUInt32();
        }

        private async Task<bool> CheckItemExisted(uint id)
        {
            var json = await _databaseService.ExecuteAsync(new QueryNative
            {
                Sql = string.Format(SqlNative.SQL_GET_ITEMEX_BY_ID, id),
                Payload = []
            });
            var items = JsonSerializer.Deserialize(json, ApplicationJsonContext.Default.ItemexEntityArray);
            return items.Length > 0;
        }
    }
}
