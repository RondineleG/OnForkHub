using HotChocolate.Execution.Configuration;
using HotChocolate.Types;

using System.Reflection;

namespace OnForkHub.CrossCutting.GraphQL;

public abstract class HotChocolateQueryBase : IGraphQLQuery
{
    public abstract string Name { get; }
    public abstract string Description { get; }

    public void Register(object descriptor)
    {
        if (descriptor is IObjectTypeDescriptor typedDescriptor)
        {
            RegisterQuery(typedDescriptor);
        }
        else
        {
            throw new ArgumentException($"Expected descriptor of type {nameof(IObjectTypeDescriptor)}");
        }
    }

    protected abstract void RegisterQuery(IObjectTypeDescriptor descriptor);
}

public abstract class HotChocolateMutationBase : IGraphQLMutation
{
    public abstract string Name { get; }
    public abstract string Description { get; }

    public void Register(object descriptor)
    {
        if (descriptor is IObjectTypeDescriptor typedDescriptor)
        {
            RegisterMutation(typedDescriptor);
        }
        else
        {
            throw new ArgumentException($"Expected descriptor of type {nameof(IObjectTypeDescriptor)}");
        }
    }

    protected abstract void RegisterMutation(IObjectTypeDescriptor descriptor);
}

public class HotChocolateSchemaBuilder(IRequestExecutorBuilder executorBuilder) : IGraphQLSchemaBuilder
{
    private readonly IRequestExecutorBuilder _executorBuilder = executorBuilder;
    private readonly List<IGraphQLQuery> _queries = [];
    private readonly List<IGraphQLMutation> _mutations = [];

    public IGraphQLSchemaBuilder AddQuery(IGraphQLQuery query)
    {
        _queries.Add(query);
        return this;
    }

    public IGraphQLSchemaBuilder AddMutation(IGraphQLMutation mutation)
    {
        _mutations.Add(mutation);
        return this;
    }

    public object Build()
    {
        _executorBuilder.AddQueryType(descriptor =>
        {
            descriptor.Name("Query");

            foreach (var query in _queries)
            {
                query.Register(descriptor);
            }
        });

        _executorBuilder.AddMutationType(descriptor =>
        {
            descriptor.Name("Mutation");

            foreach (var mutation in _mutations)
            {
                mutation.Register(descriptor);
            }
        });

        return _executorBuilder;
    }
}

public class HotChocolateConfigurator : IGraphQLConfigurator
{
    public void RegisterGraphQLServices(IServiceCollection services)
    {
        var requestExecutorBuilder = services.AddGraphQLServer();
        var hotChocolateSchemaBuilder = new HotChocolateSchemaBuilder(requestExecutorBuilder);

        RegisterOperations(hotChocolateSchemaBuilder);

        hotChocolateSchemaBuilder.Build();

        services.AddSingleton<IGraphQLSchemaBuilder>(hotChocolateSchemaBuilder);
    }

    private static void RegisterOperations(HotChocolateSchemaBuilder graphQLSchemaBuilder)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            try
            {
                foreach (var type in assembly.GetTypes()
                    .Where(t => !t.IsAbstract && typeof(IGraphQLQuery).IsAssignableFrom(t)))
                {
                    var query = (IGraphQLQuery)Activator.CreateInstance(type)!;
                    graphQLSchemaBuilder.AddQuery(query);
                }

                foreach (var type in assembly.GetTypes()
                    .Where(t => !t.IsAbstract && typeof(IGraphQLMutation).IsAssignableFrom(t)))
                {
                    var mutation = (IGraphQLMutation)Activator.CreateInstance(type)!;
                    graphQLSchemaBuilder.AddMutation(mutation);
                }
            }
            catch (ReflectionTypeLoadException)
            {
                continue;
            }
        }
    }
}