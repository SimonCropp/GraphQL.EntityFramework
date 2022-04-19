public class MultiContextSchema :
    GraphQL.Types.Schema
{
    public MultiContextSchema(IServiceProvider provider) :
        base(provider)
    {
        RegisterTypeMapping(typeof(Entity1), typeof(Entity1GraphType));
        RegisterTypeMapping(typeof(Entity2), typeof(Entity2GraphType));
        Query = (MultiContextQuery)provider.GetService(typeof(MultiContextQuery))!;
    }
}