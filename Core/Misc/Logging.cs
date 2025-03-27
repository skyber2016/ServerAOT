using System.Runtime.CompilerServices;

namespace Core;

public class Logging
{
    #region Static Methods

    public static WriteDelegate Write(Type type)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        return (value, args) =>
        {
            object finalMessage = value;
            if (args.Length > 0)
                try
                {
                    finalMessage = string.Format(value.ToString(), args);
                }
                catch (Exception)
                {
                    // Ignored.
                }

            Write(finalMessage.ToString(), type.Name);
        };
    }

    public static void Write(object value, object[] args, [CallerMemberName] string memberName = "")
    {
        object finalMessage = value;
        if (args.Length > 0)
            try
            {
                finalMessage = string.Format(value.ToString(), args);
            }
            catch (Exception)
            {
                // Ignored.
            }

        Write(finalMessage.ToString(), memberName);
    }

    internal static void LogAllExceptions()
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            if (args.ExceptionObject is Exception exception)
                Write(typeof(Logging))(exception.Message);
        };
    }

    private static void Write(string message,  string memberName)
    {
        string format = $"[{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}]: [{memberName.PadLeft(24)}] -> {message}";

        try
        {
            Console.WriteLine(format);
            using StreamWriter writer = new(Path.Combine(ApplicationInfo.AppBaseDirectory, ApplicationInfo.LogFileName), true);
            writer.WriteLine(format);
        }
        catch (Exception)
        {
            // Ignored.
        }
    }

    #endregion

    #region Nested Types, Enums, Delegates

    public delegate void WriteDelegate(object value, params object[] args);

    #endregion
}
