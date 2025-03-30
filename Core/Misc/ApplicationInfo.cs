namespace Core;

public class ApplicationInfo
{
    #region Fields

    /// <summary>
    ///     Injected Application Path
    /// </summary>
    public static readonly string AppBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

    public static readonly string PathToLog = Path.Combine(AppBaseDirectory, "Logs", $"{DateTime.Now.ToString("yyyy-MM-dd")}.log");

    public static readonly TimeSpan TimeSpanMax = TimeSpan.FromDays(365);

    #endregion
}
