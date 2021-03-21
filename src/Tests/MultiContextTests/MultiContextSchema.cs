using System;

public class MultiContextSchema :
    GraphQL.Types.Schema
{
    public MultiContextSchema(IServiceProvider provider) :
        base(provider)
    {
        RegisterTypeMapping(typeof(Entity1), typeof(Entity1Graph));
        RegisterTypeMapping(typeof(Entity2), typeof(Entity2Graph));
        Query = (MultiContextQuery)provider.GetService(typeof(MultiContextQuery))!;
    }
}