using System.Runtime.CompilerServices;

namespace Core
{
    public interface ILogger
    {
        void Critical(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string callerFile = "");
        void Debug(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string callerFile = "");
        void Error(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string callerFile = "");
        Task FlushAsync();
        void Info(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string callerFile = "");
        void Warning(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string callerFile = "");
    }
}
