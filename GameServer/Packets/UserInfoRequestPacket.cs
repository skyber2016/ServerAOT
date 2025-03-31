using Core;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Sockets;
using System.Text.Json;

namespace GameServer
{
    public class UserInfoRequestPacket : APacketHandler
    {
        private readonly ILogger _logger = LoggerManager.CreateLogger();
        private readonly IDatabaseService _databaseService;
        public UserInfoRequestPacket(Socket client, Socket proxy, Channel channel, IServiceProvider serviceProvider) : base(client, proxy, channel)
        {
            _databaseService = serviceProvider.GetRequiredService<IDatabaseService>();
        }

        protected override async Task PacketHandleAsync(UserContext context, CancellationToken cancellationToken)
        {
            _logger.Info($"{typeof(UserInfoRequestPacket)} executing...");
            var buffers = _memoryStream.ToArray();
            await this.Load(context);
            await _proxy.SendAsync(buffers);
        }

        private async Task Load(UserContext context)
        {
            _reader.ReadUInt16();
            _reader.ReadUInt16();
            context.AccountId = _reader.ReadUInt32();
            context.TokenId = _reader.ReadUInt32();
            context.CharName = await this.GetCharName(context.AccountId);
            _logger.Info($"Set user info account_id={context.AccountId} char_name={context.CharName}");
        }

        private async Task<string> GetCharName(uint accountId)
        {
            var json = await _databaseService.ExecuteAsync(new QueryNative
            {
                Sql = string.Format(SqlNative.SQL_USER_CHAR_NAME, accountId),
                Payload = []
            });
            if (json == null)
            {
                return string.Empty;
            }
            var users = JsonSerializer.Deserialize(json, ApplicationJsonContext.Default.UserEntityArray);
            return users.Select(x => x.Name).FirstOrDefault();
        }
    }
}
