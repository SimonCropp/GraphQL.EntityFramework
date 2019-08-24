using GraphQL;
using Microsoft.Extensions.DependencyInjection;

public class DependencySchema :
    GraphQL.Types.Schema
{
    public DependencySchema(ServiceProvider provider) :
        base(new FuncDependencyResolver(provider.GetRequiredService))
    {
        Query = provider.GetService<DependencyQuery>();
    }
}