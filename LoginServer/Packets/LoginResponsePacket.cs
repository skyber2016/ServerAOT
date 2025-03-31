using Core;
using System.Net.Sockets;
using System.Text;

namespace LoginServer
{
    public class LoginResponsePacket : APacketHandler
    {
        private ushort Length { get; set; }
        private ushort Type { get; set; }
        private uint AccountId { get; set; }
        private uint SessionId { get; set; }
        private uint Port { get; set; }
        private int Padding { get; set; }

        // 32 byte
        private byte[] GameSeverIP { get; set; }

        private byte[] ProxyGameServerIP => Encoding.UTF8.GetBytes(ApplicationContext.Instance.AppConfig.GameServer.LocalIP);

        private readonly ILogger _logger = LoggerManager.CreateLogger();

        public LoginResponsePacket(Socket client, Socket proxy, Channel channel) : base(client, proxy, channel)
        {
        }

        protected override async Task PacketHandleAsync(UserContext context, CancellationToken cancellationToken)
        {
            this.LoadData();
            if (IsLoginSuccess())
            {
                _logger.Info($"{_client.RemoteEndPoint} [acc_id={AccountId}] [sess={this.SessionId}] [{Encoding.UTF8.GetString(this.GameSeverIP).Trim('\0')}:{this.Port}] login success.");
                Array.Clear(GameSeverIP);
                Array.Copy(ProxyGameServerIP, 0, GameSeverIP, 0, ProxyGameServerIP.Length);
                this.Port = (uint)ApplicationContext.Instance.AppConfig.GameServer.LocalPort;
            }
            else
            {
                _logger.Info($"{_client.RemoteEndPoint} Login failed.");
            }
            var finalBuffer = this.GetBytes();
            await _client.SendAsync(finalBuffer, cancellationToken);
        }

        private void LoadData()
        {
            this.Length = _reader.ReadUInt16();
            this.Type = _reader.ReadUInt16();
            this.AccountId = _reader.ReadUInt32();
            this.SessionId = _reader.ReadUInt32();
            this.Port = _reader.ReadUInt32();
            this.Padding = _reader.ReadInt32();
            this.GameSeverIP = _reader.ReadBytes(32);
        }

        private byte[] GetBytes()
        {
            using var memoryStream = new MemoryStream();
            using var writer = new BinaryWriter(memoryStream);
            writer.Write(this.Length);
            writer.Write(this.Type);
            writer.Write(this.AccountId);
            writer.Write(this.SessionId);
            writer.Write(this.Port);
            writer.Write(this.Padding);
            writer.Write(this.GameSeverIP);
            return memoryStream.ToArray();
        }

        private bool IsLoginSuccess() => this.AccountId != 0;

    }
}
