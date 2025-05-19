// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Scripts.Logger;

public class ConsoleLogger : ILogger
{
    public void Log(ELogLevel level, string message)
    {
        var prefix = level switch
        {
            ELogLevel.Info => "[INFO]",
            ELogLevel.Error => "[ERROR]",
            ELogLevel.Debug => "[DEBUG]",
            ELogLevel.Warning => "[WARNING]",
            _ => "[INFO]",
        };
        Console.WriteLine($"{prefix} {message}");
    }
}
