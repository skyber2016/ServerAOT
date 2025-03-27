using System.Reflection;

namespace Core;

public class ApplicationInfo
{
    #region Fields

    /// <summary>
    ///     Application
    /// </summary>
    public static readonly string AssemblyPath = Assembly.GetExecutingAssembly().Location;

    /// <summary>
    ///     Injected Application Path
    /// </summary>
    public static readonly string AppBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

    /// <summary>
    ///     Log File Name
    /// </summary>
    public static readonly string LogFileName = DateTime.Now.ToString("d").Replace('/', '-') + ".log";

    #endregion
}
