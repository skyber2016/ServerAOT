using Core;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace GameServer
{
    public class NotificationService : INotificationService
    {
        private readonly IDatabaseService _databaseService;
        public NotificationService(IServiceProvider serviceProvider)
        {
            _databaseService = serviceProvider.GetRequiredService<IDatabaseService>();
        }

        public async Task SendNotification(Socket client, NotificationType type, uint accountId, string message, CancellationToken cancellation = default)
        {
            var user = await this.GetUserByAccountId(accountId, cancellation);
            if (user == null) return;
            var paddings = new List<byte> { 0xEC, 0x03, 0x00, 0xFF, 0xFF, 0x00, 0xD5, 0x07, 0x00, 0x00, 0x64, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x06, 0x53, 0x59, 0x53, 0x54, 0x45, 0x4D };
            var msg = Encoding.UTF8.GetBytes(message);
            var charName = Encoding.UTF8.GetBytes(user.Name);
            var offsets = new byte[] { 0x00, 0x4F };
            paddings.Add((byte)charName.Length);
            paddings.AddRange(charName);
            paddings.AddRange(offsets);
            paddings.Add((byte)msg.Length);
            paddings.AddRange(msg);
            using var mem = new MemoryStream();
            using var writer = new BinaryWriter(mem);
            writer.Write((ushort)(paddings.Count + 2 + 3));
            writer.Write(paddings.ToArray());
            writer.Write(byte.MinValue);
            writer.Write(byte.MinValue);
            writer.Write(byte.MinValue);
            var final = mem.ToArray();
            await client.SendAsync(final);
        }


        private async Task<UserEntity> GetUserByAccountId(uint accountId, CancellationToken cancellationToken = default)
        {
            var json = await _databaseService.ExecuteAsync(new QueryNative
            {
                Sql = string.Format(SqlNative.SQL_USER_CHAR_NAME, accountId),
                Payload = []
            }, cancellationToken);
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }
            return JsonSerializer.Deserialize(json, ApplicationJsonContext.Default.UserEntityArray).FirstOrDefault();
        }
    }
}
