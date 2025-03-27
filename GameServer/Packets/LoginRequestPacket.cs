using System.Net.Sockets;
using System.Text;
using Core;
using static Core.Logging;

namespace GameServer
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

        private readonly WriteDelegate _logger = Write(typeof(LoginRequestPacket));

        public LoginRequestPacket(Socket client, Socket proxy, Channel channel) : base(client, proxy, channel)
        {
        }

        protected override Task PacketHandleAsync(CancellationToken cancellationToken)
        {
            this.LoadData();
            _logger($"Username: {Username} send request login...");
            return Task.CompletedTask;
        }

        private void LoadData()
        {
            this.Length = _reader.ReadUInt16();
            this.Type = _reader.ReadUInt16();
            this.Padding = _reader.ReadInt32();
            this.Username = Encoding.Unicode.GetString(_reader.ReadBytes(32));
            this.Password = Encoding.Unicode.GetString(_reader.ReadBytes(32));
            this.ServerName = Encoding.Unicode.GetString(_reader.ReadBytes(32));
            this.Version = Encoding.Unicode.GetString(_reader.ReadBytes(32));
        }

    }
}
