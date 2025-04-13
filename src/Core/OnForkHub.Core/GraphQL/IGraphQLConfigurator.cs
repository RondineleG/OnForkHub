using Microsoft.Extensions.DependencyInjection;

namespace OnForkHub.Core.GraphQL;

public interface IGraphQLConfigurator
{
    void ConfigureGraphQL(IServiceCollection services);
}