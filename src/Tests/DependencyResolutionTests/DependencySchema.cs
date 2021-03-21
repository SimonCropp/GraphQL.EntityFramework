using Microsoft.Extensions.DependencyInjection;

public class DependencySchema :
    GraphQL.Types.Schema
{
    public DependencySchema(ServiceProvider provider) :
        base(provider)
    {
        GraphTypeTypeRegistry.Register<Entity, EntityGraph>();
        Query = provider.GetService<DependencyQuery>();
    }
}