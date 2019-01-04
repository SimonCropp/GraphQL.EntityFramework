using System;
using GraphQL.Types.Relay;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.EntityFramework
{
    public static class EfGraphQLConventions
    {
        public static void RegisterInContainer(Action<Type, object> register, DbContext context)
        {
            Guard.AgainstNull(nameof(register), register);
            Guard.AgainstNull(nameof(context), context);
            Scalars.RegisterInContainer(register);
            ArgumentGraphs.RegisterInContainer(register);

            var service = new EfGraphQLService(context);
            register(typeof(IEfGraphQLService), service);
        }

        public static void RegisterInContainer(IServiceCollection services, DbContext context)
        {
            Guard.AgainstNull(nameof(services), services);
            Guard.AgainstNull(nameof(context), context);
            services.AddTransient(typeof(ConnectionType<>));
            services.AddTransient(typeof(EdgeType<>));
            services.AddSingleton<PageInfoType>();
            RegisterInContainer((type, instance) => { services.AddSingleton(type, instance); }, context);
        }

        [Obsolete("No longer required. Done as part of EfGraphQLConventions.RegisterInContainer", true)]
        public static void RegisterConnectionTypesInContainer(IServiceCollection services)
        {
            Guard.AgainstNull(nameof(services), services);
            services.AddTransient(typeof(ConnectionType<>));
            services.AddTransient(typeof(EdgeType<>));
            services.AddSingleton<PageInfoType>();
        }

        public static void RegisterConnectionTypesInContainer(Action<Type> register)
        {
            Guard.AgainstNull(nameof(register), register);
            register(typeof(ConnectionType<>));
            register(typeof(EdgeType<>));
            register(typeof(PageInfoType));
        }
    }
}