using System;

public class MultiContextSchema :
    GraphQL.Types.Schema
{
    public MultiContextSchema(IServiceProvider provider) :
        base(provider)
    {
        Query = (MultiContextQuery)provider.GetService(typeof(MultiContextQuery));
    }
}