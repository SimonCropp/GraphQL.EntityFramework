using System;

public class Schema :
    GraphQL.Types.Schema
{
    public Schema(IServiceProvider provider, Query query) :
        base(provider)
    {
        Query = query;
        //   Subscription = (Subscription)provider.GetService(typeof(Subscription));
    }
}