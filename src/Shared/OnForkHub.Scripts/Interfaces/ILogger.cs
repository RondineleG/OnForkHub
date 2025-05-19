// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Scripts.Interfaces;

public interface ILogger
{
    void Log(ELogLevel level, string message);
}
