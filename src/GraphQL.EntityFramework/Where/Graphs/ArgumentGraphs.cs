using GraphQL.EntityFramework;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

static class ArgumentGraphs
{
    static Dictionary<Type, GraphType> entries = new();

    [ModuleInitializer]
    public static void Initialize()
    {
        Add<EnumerationGraphType<StringComparison>>();
        Add<WhereExpressionGraph>();
        Add<OrderByGraph>();
        Add<ComparisonGraph>();
        Add<ConnectorGraph>();
    }

    public static void RegisterInContainer(IServiceCollection services)
    {
        foreach (var entry in entries)
        {
            services.AddSingleton(entry.Key, entry.Value);
        }
    }

    static void Add<T>()
        where T : GraphType, new()
    {
        var value = new T();
        entries.Add(typeof(T), value);
    }
}