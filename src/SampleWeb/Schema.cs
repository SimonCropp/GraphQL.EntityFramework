using System;

public class Schema :
    GraphQL.Types.Schema
{
    public Schema(IServiceProvider provider) :
        base(provider)
    {
        Query = (Query)provider.GetService(typeof(Query));
     //   Subscription = (Subscription)provider.GetService(typeof(Subscription));
    }
}