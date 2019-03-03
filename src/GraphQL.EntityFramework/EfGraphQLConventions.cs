using System;
using GraphQL.Types.Relay;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.EntityFramework
{
    public static class EfGraphQLConventions
    {
        public static void RegisterInContainer(Action<Type, object> register, DbContext context, GlobalFilters filters = null)
        {
            Guard.AgainstNull(nameof(register), register);
            Guard.AgainstNull(nameof(context), context);
            Scalars.RegisterInContainer(register);
            ArgumentGraphs.RegisterInContainer(register);

            if (filters == null)
            {
                filters = new GlobalFilters();
            }

            var service = new EfGraphQLService(context, filters);
            register(typeof(IEfGraphQLService), service);
        }

        public static void RegisterInContainer(IServiceCollection services, DbContext context, GlobalFilters filters = null)
        {
            Guard.AgainstNull(nameof(services), services);
            Guard.AgainstNull(nameof(context), context);
            services.AddTransient(typeof(ConnectionType<>));
            services.AddTransient(typeof(EdgeType<>));
            services.AddSingleton<PageInfoType>();
            RegisterInContainer((type, instance) => { services.AddSingleton(type, instance); }, context, filters);
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