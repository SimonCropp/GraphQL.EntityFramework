using System;
using GraphQL.Types;
using GraphQL.Types.Relay;
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

        public static void RegisterConnectionTypesInContainer(IServiceCollection services)
        {
            services.AddTransient(typeof(ConnectionType<>));
            services.AddTransient(typeof(EdgeType<>));
            services.AddSingleton<PageInfoType>();
        }

        public static void RegisterConnectionTypesInContainer(Action<Type> register)
        {
            register(typeof(ConnectionType<>));
            register(typeof(EdgeType<>));
            register(typeof(PageInfoType));
        }
    }
}