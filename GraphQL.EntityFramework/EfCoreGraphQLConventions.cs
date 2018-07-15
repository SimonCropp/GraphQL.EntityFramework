using System;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.EntityFramework
{
    public static class EfCoreGraphQLConventions
    {
        public static void RegisterInContainer(Action<Type, GraphType> registerInstance)
        {
            Scalars.RegisterInContainer(registerInstance);
            ArgumentGraphs.RegisterInContainer(registerInstance);
        }
        public static void RegisterInContainer(IServiceCollection services)
        {
            RegisterInContainer((type, instance) => { services.AddSingleton(type, instance); });
        }
    }
}