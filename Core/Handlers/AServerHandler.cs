using System.Net;
using System.Net.Sockets;

namespace Core
{
    public abstract class AServerHandler : IDisposable
    {
        protected abstract int Port { get; }
        protected abstract Task OnConnected(Socket client, CancellationToken cancellationToken);

        protected readonly Socket _serverSocket;
        private ILogger _logger = LoggerManager.CreateLogger();
        public readonly string _requestId = Guid.NewGuid().ToString("N");
        protected AServerHandler()
        {
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        }

        public void Init()
        {
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
            _serverSocket.Listen(1000);
            _logger.Info($"Server started on port {Port}");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Socket client = await _serverSocket.AcceptAsync(cancellationToken);
                _logger.Info($"Client {client.RemoteEndPoint} connected");
                await this.OnConnected(client, cancellationToken);
            }
        }

        public virtual void Dispose()
        {
            if (_serverSocket != null)
            {
                _serverSocket.Shutdown(SocketShutdown.Both);
                _serverSocket.Dispose();
                _logger.Info("Disposing server");
            }
            _logger.Info("Server stopped");
        }
    }
}
