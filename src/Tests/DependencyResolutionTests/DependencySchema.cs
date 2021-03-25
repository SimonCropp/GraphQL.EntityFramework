using System;
using Microsoft.Extensions.DependencyInjection;

public class DependencySchema :
    GraphQL.Types.Schema
{
    public DependencySchema(IServiceProvider provider) :
        base(provider)
    {
        RegisterTypeMapping(typeof(Entity), typeof(EntityGraph));
        Query = provider.GetService<DependencyQuery>();
    }
}