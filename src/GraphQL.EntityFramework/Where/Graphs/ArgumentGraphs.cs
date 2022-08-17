using GraphQL.EntityFramework;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

static class ArgumentGraphs
{
    public static void RegisterInContainer(IServiceCollection services)
    {
       services.AddSingleton<EnumerationGraphType<StringComparison>>();
       services.AddSingleton<WhereExpressionGraph>();
       services.AddSingleton<OrderByGraph>();
       services.AddSingleton<ComparisonGraph>();
       services.AddSingleton<ConnectorGraph>();
    }
}