using System.Runtime.CompilerServices;

namespace OnForkHub.Application.DependencyInjection;

internal static class LoggerExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Let<T>(this T? obj, Action<T> action)
        where T : class
    {
        if (obj is not null)
            action(obj);
    }
}
