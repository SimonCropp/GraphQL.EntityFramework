using System;

public class Schema :
    GraphQL.Types.Schema
{
    public Schema(IServiceProvider resolver) :
        base(resolver)
    {
        Query = (Query)resolver.GetService(typeof(Query));
        Mutation = (Mutation)resolver.GetService(typeof(Mutation));
        RegisterType<DerivedGraph>();
        RegisterType<DerivedWithNavigationGraph>();
    }
}