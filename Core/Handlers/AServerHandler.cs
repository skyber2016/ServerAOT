using System.Net;
using System.Net.Sockets;
using static Core.Logging;

namespace Core.Providers
{
    public abstract class AServerHandler : IDisposable
    {
        protected abstract int Port { get; }
        protected abstract Task OnConnected(Socket client, CancellationToken cancellationToken);

        protected readonly Socket _severSocket;
        protected WriteDelegate _logger = Logging.Write(typeof(AServerHandler));

        protected AServerHandler()
        {
            _severSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Init()
        {
            _severSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
            _severSocket.Listen();
            _logger($"Server started on port {Port}");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Socket client = await _severSocket.AcceptAsync(cancellationToken);
                _logger($"Client {client.RemoteEndPoint} connected");
                await this.OnConnected(client, cancellationToken);
            }
        }

        public virtual void Dispose()
        {
            if(_severSocket != null)
            {
                _severSocket.Dispose();
                _logger("Disposing server");
            }
            _logger("Server stopped");
        }
    }
}
