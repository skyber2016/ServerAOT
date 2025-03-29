using System.Net;
using System.Net.Sockets;

namespace Core
{
    public abstract class AServerHandler : IDisposable
    {
        protected abstract int Port { get; }
        protected abstract Task OnConnected(Socket client, CancellationToken cancellationToken);

        protected readonly Socket _severSocket;
        private ILogger _logger = LoggerManager.CreateLogger();
        public readonly string _requestId = Guid.NewGuid().ToString("N");
        protected AServerHandler()
        {
            _severSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Init()
        {
            _severSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
            _severSocket.Listen();
            _logger.Info($"Server started on port {Port}");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Socket client = await _severSocket.AcceptAsync(cancellationToken);
                _logger.Info($"Client {client.RemoteEndPoint} connected");
                await this.OnConnected(client, cancellationToken);
            }
        }

        public virtual void Dispose()
        {
            if (_severSocket != null)
            {
                _severSocket.Dispose();
                _logger.Info("Disposing server");
            }
            _logger.Info("Server stopped");
        }
    }
}
