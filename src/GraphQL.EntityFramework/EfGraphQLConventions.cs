using System;
using GraphQL.Types.Relay;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.EntityFramework
{
    public static class EfGraphQLConventions
    {
        #region RegisterInContainerAction
        public static void RegisterInContainer<TDbContext>(Action<Type, object> register, IModel model, GlobalFilters filters = null)
            where TDbContext : DbContext
        #endregion
        {
            Guard.AgainstNull(nameof(register), register);
            Guard.AgainstNull(nameof(model), model);
            Scalars.RegisterInContainer(register);
            ArgumentGraphs.RegisterInContainer(register);

            if (filters == null)
            {
                filters = new GlobalFilters();
            }

            var service = new EfGraphQLService<TDbContext>(model, filters);
            register(typeof(IEfGraphQLService<TDbContext>), service);
        }

        #region RegisterInContainerServiceCollection
        public static void RegisterInContainer<TDbContext>(IServiceCollection services, IModel model, GlobalFilters filters = null)
            where TDbContext : DbContext
        #endregion
        {
            Guard.AgainstNull(nameof(services), services);
            Guard.AgainstNull(nameof(model), model);
            services.AddTransient(typeof(ConnectionType<>));
            services.AddTransient(typeof(EdgeType<>));
            services.AddSingleton<PageInfoType>();
            RegisterInContainer<TDbContext>((type, instance) => { services.AddSingleton(type, instance); }, model, filters);
        }

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