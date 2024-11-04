namespace OnForkHub.Core.Abstractions.Base;

public interface IRequestCustomResult<out T> : IRequestResult
{
    T? Data { get; }
}
