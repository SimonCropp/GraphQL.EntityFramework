using Microsoft.Extensions.DependencyInjection;

public class DependencySchema :
    GraphQL.Types.Schema
{
    public DependencySchema(ServiceProvider provider) :
        base(provider)
    {
        Query = provider.GetService<DependencyQuery>();
    }
}