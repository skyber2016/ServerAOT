using System.Net.Sockets;
using static Core.Logging;

namespace Core
{
    public abstract class APacketHandler : IDisposable
    {
        protected BinaryReader _reader { get; set; }
        protected BinaryWriter _writer { get; set; }
        protected MemoryStream _memoryStream { get; set; }
        protected readonly Socket _client;
        protected readonly Socket _proxy;
        protected readonly Channel _channel;

        private readonly WriteDelegate _logger = Write(typeof(APacketHandler));
        protected abstract Task PacketHandleAsync(CancellationToken cancellationToken);
        protected APacketHandler(Socket client, Socket proxy, Channel channel)
        {
            _client = client;
            _proxy = proxy;
            _channel = channel;
        }
        public void Load(byte[] buffer)
        {
            _memoryStream = new MemoryStream(buffer);
            _reader = new BinaryReader(_memoryStream);
            _writer = new BinaryWriter(_memoryStream);
        }

        public async Task<bool> HandleAsync(CancellationToken cancellationToken)
        {
            try
            {
                await HandleAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger("Exception when handle packet. Exception: " + ex.GetBaseException().Message);
                return false;
            }
        }

        public void Dispose()
        {
            this.Cleanup();
        }

        private void Cleanup()
        {
            _memoryStream?.Dispose();
            _reader?.Dispose();
            _writer?.Dispose();
        }
    }
}
