// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Core.Interfaces.Requests;

public interface IRequestCustomResult<out T> : IRequestResult
{
    T? Data { get; }
}
