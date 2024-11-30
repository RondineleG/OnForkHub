using OnForkHub.Scripts.Enums;

namespace OnForkHub.Scripts.Interfaces;

public interface ILogger
{
    void Log(ELogLevel level, string message);
}
