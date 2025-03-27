using System.Net.Sockets;
using static Core.Logging;

namespace Core;

public abstract class AProxyHandler : IDisposable
{
    protected readonly Socket _clientSocket;
    protected readonly Socket _proxySocket;
    private readonly WriteDelegate _logger = Write(typeof(AProxyHandler));

    protected abstract Task OnReceived(byte[] buffer, Channel channel, CancellationToken cancellationToken);

    protected abstract int Port { get; }
    protected abstract string IpProxy { get; }

    public AProxyHandler(Socket clientSocket)
    {
        _clientSocket = clientSocket;
        _proxySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    public async Task<bool> InitAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _proxySocket.ConnectAsync(IpProxy, Port, cancellationToken);
            _logger($"{_clientSocket.RemoteEndPoint} Connected to proxy {IpProxy}:{Port}");
            return true;
        }
        catch (Exception ex)
        {
            _logger("Exception when init proxy. Exception: " + ex.GetBaseException());
            return false;
        }
    }

    public async Task HandleAsync(CancellationToken cancellationToken)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        using var clientNetwork = new NetworkStream(_clientSocket, true);
        using var proxyNetwork = new NetworkStream(_proxySocket, true);
        var tasks = new[]
        {
            HandleNetworkAsync(clientNetwork, Channel.C2S, cancellationTokenSource.Token),
            HandleNetworkAsync(proxyNetwork, Channel.S2C, cancellationTokenSource.Token),
            Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(100);
                }
            }, cancellationToken)
        };
        await Task.WhenAny(tasks);
        cancellationTokenSource.Cancel();
    }

    private async Task HandleNetworkAsync(NetworkStream network, Channel channel, CancellationToken cancellationToken)
    {
        var socket = network.Socket;
        var endpoint = channel == Channel.C2S ? socket.RemoteEndPoint : socket.LocalEndPoint;
        try
        {
            byte[] buffer = new byte[1024 * 100];
            while (!cancellationToken.IsCancellationRequested)
            {
                var totalBytes = await network.ReadAsync(buffer, cancellationToken);
                if (totalBytes == 0)
                {
                    _logger($"Connection {endpoint} closed");
                    break;
                }
                var finalBuffer = new byte[totalBytes];
                Array.Copy(buffer, finalBuffer, totalBytes);
                await this.OnReceived(finalBuffer, channel, cancellationToken);
                Array.Clear(buffer);
            }
        }
        catch (SocketException ex)
        {
            _logger($"Client {endpoint} disconnected (socket error): {ex.GetBaseException().Message}");
        }
        catch (Exception ex)
        {
            _logger($"Client {endpoint} error: {ex.GetBaseException().Message}");
        }

    }

    public virtual void Dispose()
    {
        _clientSocket?.Dispose();
        _proxySocket?.Dispose();
        _logger($"Proxy server disconnected");
    }
}
