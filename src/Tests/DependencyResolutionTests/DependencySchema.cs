using Microsoft.Extensions.DependencyInjection;

public class DependencySchema :
    GraphQL.Types.Schema
{
    public DependencySchema(IServiceProvider provider) :
        base(provider)
    {
        RegisterTypeMapping(typeof(Entity), typeof(EntityGraphType));
        Query = provider.GetRequiredService<DependencyQuery>();
    }
}