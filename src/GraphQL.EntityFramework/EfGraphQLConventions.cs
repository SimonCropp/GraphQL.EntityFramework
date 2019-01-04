using System;
using GraphQL.Types.Relay;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.EntityFramework
{
    public static class EfGraphQLConventions
    {
        public static void RegisterInContainer(Action<Type, object> registerInstance, DbContext dbContext)
        {
            Guard.AgainstNull(nameof(registerInstance), registerInstance);
            Guard.AgainstNull(nameof(dbContext), dbContext);
            Scalars.RegisterInContainer(registerInstance);
            ArgumentGraphs.RegisterInContainer(registerInstance);

            var service = new EfGraphQLService(dbContext);
            registerInstance(typeof(IEfGraphQLService), service);
        }

        public static void RegisterInContainer(IServiceCollection services, DbContext dbContext)
        {
            Guard.AgainstNull(nameof(services), services);
            Guard.AgainstNull(nameof(dbContext), dbContext);
            services.AddTransient(typeof(ConnectionType<>));
            services.AddTransient(typeof(EdgeType<>));
            services.AddSingleton<PageInfoType>();
            RegisterInContainer((type, instance) => { services.AddSingleton(type, instance); }, dbContext);
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