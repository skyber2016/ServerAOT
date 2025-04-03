using Core;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Sockets;

namespace GameServer
{
    public class UserInfoPacket : APacketHandler
    {
        private readonly ILogger _logger = LoggerManager.CreateLogger();
        private readonly IDatabaseService _databaseService;
        private readonly IMemoryCacheService _memoryService;
        public UserInfoPacket(Socket client, Socket proxy, Channel channel, IServiceProvider serviceProvider) : base(client, proxy, channel)
        {
            _databaseService = serviceProvider.GetRequiredService<IDatabaseService>();
            _memoryService = ApplicationContext.Instance.Caching;
        }

        protected override async Task PacketHandleAsync(UserContext context, CancellationToken cancellationToken)
        {
            var buffers = _memoryStream.ToArray();
            if(_channel == Channel.C2S)
            {
                await _proxy.SendAsync(buffers);
                return;
            }
            _reader.ReadUInt32();
            var itemId = _reader.ReadUInt32();
            if (_memoryService.TryGetValue(itemId.ToString(), out bool isDeleted))
            {
                _logger.Info($"item_id={itemId} has existed in deleted list.");
                if (!isDeleted)
                {
                    _logger.Info($"Deleting item_id={itemId}");
                    await this.DeleteItem(itemId);
                    _memoryService.Set(itemId.ToString(), true, TimeSpan.MaxValue);
                    _logger.Info($"Marked item_id={itemId} deleted");
                }
                
            }
            await _client.SendAsync(buffers);
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

    }
}
