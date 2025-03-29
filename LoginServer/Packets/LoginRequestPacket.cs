using System.Net.Sockets;
using System.Text;
using Core;

namespace LoginServer
{
    public class LoginRequestPacket : APacketHandler
    {
        private ushort Length { get; set; }
        private ushort Type { get; set; }
        private int Padding { get; set; }
        private string Username { get; set; }
        private string Password { get; set; }
        private string ServerName { get; set; }
        private string Version { get; set; }

        private readonly ILogger _logger = LoggerManager.CreateLogger();

        public LoginRequestPacket(Socket client, Socket proxy, Channel channel) : base(client, proxy, channel)
        {
        }

        protected override async Task PacketHandleAsync(UserContext context, CancellationToken cancellationToken)
        {
            this.LoadData();
            _logger.Info($"Username: {Username} send request login...");
            var source = _memoryStream.ToArray();
            await _proxy.SendAsync(source, cancellationToken);
        }

        private void LoadData()
        {
            this.Length = _reader.ReadUInt16();
            this.Type = _reader.ReadUInt16();
            this.Padding = _reader.ReadInt32();
            this.Username = Encoding.UTF8.GetString(_reader.ReadBytes(32)).Trim('\0');
            this.Password = Encoding.UTF8.GetString(_reader.ReadBytes(32)).Trim('\0');
            this.ServerName = Encoding.UTF8.GetString(_reader.ReadBytes(32)).Trim('\0');
            this.Version = Encoding.UTF8.GetString(_reader.ReadBytes(32)).Trim('\0');
        }

    }
}
