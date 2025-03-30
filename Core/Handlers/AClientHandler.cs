using System.Net.Sockets;

namespace Core;

public abstract class AClientHandler : IDisposable
{
    public readonly Socket _clientSocket;
    public readonly Socket _serverSocket;

    public readonly string _requestId = Guid.NewGuid().ToString("N");

    private readonly ILogger _logger = LoggerManager.CreateLogger();

    protected abstract Task OnReceived(byte[] buffer, Channel channel, CancellationToken cancellationToken);

    protected abstract int ServerPort { get; }
    protected abstract string ServerIP { get; }

    public AClientHandler(Socket clientSocket)
    {
        _clientSocket = clientSocket;
        _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    public async Task<bool> InitAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _serverSocket.ConnectAsync(ServerIP, ServerPort, cancellationToken);
            _logger.Info($"{_clientSocket.RemoteEndPoint} Connected to proxy {ServerIP}:{ServerPort}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error("Exception when init proxy. Exception: " + ex.GetBaseException());
            return false;
        }
    }

    public async Task HandleAsync(CancellationToken cancellationToken)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var tasks = new[]
        {
            HandleNetworkAsync(_clientSocket, Channel.C2S, cancellationTokenSource.Token),
            HandleNetworkAsync(_serverSocket, Channel.S2C, cancellationTokenSource.Token),
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

    private async Task HandleNetworkAsync(Socket network, Channel channel, CancellationToken cancellationToken)
    {
        try
        {
            byte[] buffers = new byte[1024 * 100];
            while (!cancellationToken.IsCancellationRequested)
            {
                var totalBytes = await network.ReceiveAsync(buffers, cancellationToken);
                if (totalBytes == 0)
                {
                    _logger.Info($"Connection closed");
                    break;
                }
                int sourceIndex = 0;
                while (GetNextBuffer(buffers, sourceIndex, out byte[] finalBuffer))
                {
                    sourceIndex += finalBuffer.Length;
                    await this.OnReceived(finalBuffer, channel, cancellationToken);
                }

                Array.Clear(buffers);
            }
        }
        catch (SocketException ex)
        {
            _logger.Error($"Client disconnected (socket error): {ex.GetBaseException().Message}");
        }
        catch (Exception ex)
        {
            _logger.Error($"Client error: {ex.GetBaseException().Message}");
        }

    }

    private bool GetNextBuffer(byte[] buffers, int index, out byte[] finalBuffer)
    {
        finalBuffer = [];

        // Kiểm tra buffer hợp lệ
        if (buffers == null || buffers.Length < 2)
        {
            _logger.Error("Error: Input buffer is null or too small.");
            return false;
        }

        // Kiểm tra index hợp lệ
        if (index < 0 || index >= buffers.Length - 1)
        {
            _logger.Error("Error: Index is out of range.");
            return false;
        }

        // Lấy độ dài packet từ 2 byte đầu tiên
        uint packetLength = BitConverter.ToUInt16(buffers, index);
        if(packetLength == 0)
        {
            return false;
        }
        // Kiểm tra packetLength hợp lệ
        if (packetLength == 0 || packetLength > buffers.Length - index)
        {
            _logger.Error("Error: Invalid packet length.");
            return false;
        }

        // Sao chép dữ liệu
        finalBuffer = new byte[packetLength];
        Array.Copy(buffers, index, finalBuffer, 0, packetLength);

        return true;
    }


    public virtual void Dispose()
    {
        _clientSocket?.Dispose();
        _serverSocket?.Dispose();
        _logger.Info($"Proxy server disconnected");
    }
}
