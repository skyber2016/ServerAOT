using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core;

public class LoggerManager : ILogger, IDisposable
{
    private string _filePath => ApplicationInfo.PathToLog;
    private readonly ConcurrentQueue<string> _logQueue = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _logTask;
    private readonly LogLevel _minLogLevel;
    private string _requestId { get; set; }

    public LoggerManager()
    {
        _requestId = "NONE";
        _minLogLevel = LogLevel.Info;
        _logTask = Task.Run(ProcessQueueAsync);
    }
    private static Lazy<ILogger> _logger = new(() => new LoggerManager());
    public static ILogger CreateLogger() => _logger.Value;

    public void SetRequestId(string requestId)
    {
        _requestId = requestId;
    }

    private void Log(LogLevel level, string message, [CallerMemberName] string memberName = "")
    {
        if (level < _minLogLevel) return;

        string logEntry = $"[{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}] [{level}] [{memberName,24}] [req_{_requestId}] -> {message}";
        _logQueue.Enqueue(logEntry);
    }

    public void Debug(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string callerFile = "") => Log(LogLevel.Debug, message, $"{Path.GetFileNameWithoutExtension(callerFile)}.{memberName}");
    public void Info(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string callerFile = "") => Log(LogLevel.Info, message, $"{Path.GetFileNameWithoutExtension(callerFile)}.{memberName}");
    public void Warning(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string callerFile = "") => Log(LogLevel.Warning, message, $"{Path.GetFileNameWithoutExtension(callerFile)}.{memberName}");
    public void Error(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string callerFile = "") => Log(LogLevel.Error, message, $"{Path.GetFileNameWithoutExtension(callerFile)}.{memberName}");
    public void Critical(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string callerFile = "") => Log(LogLevel.Critical, message, $"{Path.GetFileNameWithoutExtension(callerFile)}.{memberName}");

    private async Task ProcessQueueAsync()
    {
        using StreamWriter writer = new(_filePath, append: true, Encoding.UTF8);
        while (!_cts.Token.IsCancellationRequested || !_logQueue.IsEmpty)
        {
            while (_logQueue.TryDequeue(out var logEntry))
            {
                Console.WriteLine(logEntry);
                await writer.WriteLineAsync(logEntry);
            }
            await writer.FlushAsync();
            await Task.Delay(100);
        }
    }

    public async Task FlushAsync()
    {
        using StreamWriter writer = new(_filePath, append: true, Encoding.UTF8);
        while (_logQueue.TryDequeue(out var logEntry))
        {
            Console.WriteLine(logEntry);
            await writer.WriteLineAsync(logEntry);
        }
        await writer.FlushAsync();
    }

    public async void Dispose()
    {
        _cts.Cancel();
        await _logTask;
        await FlushAsync();
    }
}