using System;
using Microsoft.Extensions.DependencyInjection;

public class MultiContextSchema :
    GraphQL.Types.Schema
{
    public MultiContextSchema(IServiceProvider resolver) :
        base(resolver)
    {
        Query = resolver.GetService<MultiContextQuery>();
    }
}