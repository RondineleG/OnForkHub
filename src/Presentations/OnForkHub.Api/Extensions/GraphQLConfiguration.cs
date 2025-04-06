namespace OnForkHub.Api.Extensions;

public static class GraphQLConfiguration
{
    public static IServiceCollection AddGraphQLServices(this IServiceCollection services)
    {
        services.AddGraphQLServer().AddMutationType<CreateCategoryMutation>().ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true);

        return services;
    }
}
