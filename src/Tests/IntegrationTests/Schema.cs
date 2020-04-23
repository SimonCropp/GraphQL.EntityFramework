using System;
using Microsoft.Extensions.DependencyInjection;

public class Schema :
    GraphQL.Types.Schema
{
    public Schema(IServiceProvider resolver) :
        base(resolver)
    {
        Query = resolver.GetService<Query>();
    }
}