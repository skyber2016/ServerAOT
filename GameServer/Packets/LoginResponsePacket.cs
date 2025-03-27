using System.Net.Sockets;
using System.Text;
using Core;
using static Core.Logging;

namespace GameServer
{
    public class LoginResponsePacket : APacketHandler
    {
        private ushort Length { get; set; }
        private ushort Type { get; set; }
        private uint AccountId { get; set; }
        private uint UserId { get; set; }
        private uint Port { get; set; }
        private int Padding { get; set; }

        // 32 byte
        private byte[] GameSeverIP { get; set; }

        private byte[] ProxyGameServerIP => Encoding.UTF8.GetBytes("127.0.0.1");

        private readonly WriteDelegate _logger = Write(typeof(LoginResponsePacket));

        public LoginResponsePacket(Socket client, Socket proxy, Channel channel) : base(client, proxy, channel)
        {
        }

        protected override async Task PacketHandleAsync(CancellationToken cancellationToken)
        {
            this.LoadData();
            if(IsLoginSuccess())
            {
                _logger($"{_client.RemoteEndPoint} [acc_id={AccountId}] login success.");
                Array.Copy(ProxyGameServerIP, 0, GameSeverIP, 0, ProxyGameServerIP.Length);
            }
            else
            {
                _logger($"{_client.RemoteEndPoint} Login failed.");
            }
            var finalBuffer = this.GetBytes();
            await _client.SendAsync(finalBuffer, cancellationToken);
        }

        private void LoadData()
        {
            this.Length = _reader.ReadUInt16();
            this.Type = _reader.ReadUInt16();
            this.AccountId = _reader.ReadUInt32();
            this.UserId = _reader.ReadUInt32();
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
            writer.Write(this.UserId);
            writer.Write(this.Port);
            writer.Write(this.Padding);
            writer.Write(this.GameSeverIP);
            return memoryStream.ToArray();
        }

        private bool IsLoginSuccess() => this.AccountId != 0;

    }
}
