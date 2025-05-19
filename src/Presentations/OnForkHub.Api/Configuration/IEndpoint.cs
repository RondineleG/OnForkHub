// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Api.Configuration;

public interface IEndpoint
{
    void Register(WebApplication app);
}
