using System;
using GraphQL.Types.Relay;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.EntityFramework
{
    public static class EfGraphQLConventions
    {
        public static void RegisterInContainer(Action<Type, object> register, IModel model, GlobalFilters filters = null)
        {
            Guard.AgainstNull(nameof(register), register);
            Guard.AgainstNull(nameof(model), model);
            Scalars.RegisterInContainer(register);
            ArgumentGraphs.RegisterInContainer(register);

            if (filters == null)
            {
                filters = new GlobalFilters();
            }

            var service = new EfGraphQLService(model, filters);
            register(typeof(IEfGraphQLService), service);
        }

        public static void RegisterInContainer(IServiceCollection services, IModel model, GlobalFilters filters = null)
        {
            Guard.AgainstNull(nameof(services), services);
            Guard.AgainstNull(nameof(model), model);
            services.AddTransient(typeof(ConnectionType<>));
            services.AddTransient(typeof(EdgeType<>));
            services.AddSingleton<PageInfoType>();
            RegisterInContainer((type, instance) => { services.AddSingleton(type, instance); }, model, filters);
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