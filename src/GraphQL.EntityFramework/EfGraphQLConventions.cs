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
        public static void RegisterInContainer<TDbContext>(
            Action<Type, object> register,
            TDbContext dbContext,
            DbContextFromUserContext<TDbContext> dbContextFromUserContext,
            GlobalFilters filters = null)
            where TDbContext : DbContext
        #endregion
        {
            Guard.AgainstNull(nameof(register), register);
            Guard.AgainstNull(nameof(dbContextFromUserContext), dbContextFromUserContext);
            Guard.AgainstNull(nameof(dbContext), dbContext);
            Scalars.RegisterInContainer(register);
            ArgumentGraphs.RegisterInContainer(register);

            if (filters == null)
            {
                filters = new GlobalFilters();
            }

            var service = new EfGraphQLService<TDbContext>(dbContext.Model, filters,dbContextFromUserContext);
            register(typeof(IEfGraphQLService<TDbContext>), service);
        }

        #region RegisterInContainerServiceCollection
        public static void RegisterInContainer<TDbContext>(
            IServiceCollection services,
            TDbContext dbContext,
            DbContextFromUserContext<TDbContext> dbContextFromUserContext,
            GlobalFilters filters = null)
            where TDbContext : DbContext
        #endregion
        {
            Guard.AgainstNull(nameof(services), services);
            Guard.AgainstNull(nameof(dbContext), dbContext);
            services.AddTransient(typeof(ConnectionType<>));
            services.AddTransient(typeof(EdgeType<>));
            services.AddSingleton<PageInfoType>();
            RegisterInContainer((type, instance) => { services.AddSingleton(type, instance); }, dbContext, dbContextFromUserContext, filters);
        }

        public static void RegisterInContainer<TDbContext>(
            IServiceCollection services,
            DbContextFromUserContext<TDbContext> dbContextFromUserContext,
            Func<IServiceProvider, IModel> dbModelCreator = null,
            Func<IServiceProvider, GlobalFilters> filters = null)
            where TDbContext : DbContext
        {
            Guard.AgainstNull(nameof(services), services);
            //register connection types
            services.AddTransient(typeof(ConnectionType<>));
            services.AddTransient(typeof(EdgeType<>));
            services.AddSingleton<PageInfoType>();
            //acquire the database model via the service provider
            //default implmentation is below, but can be tailored by the caller
            if (dbModelCreator == null)
            {
                dbModelCreator = (serviceProvider) =>
                {
                    //create a scope, as EfGraphQLService is a singleton, and databases are scoped
                    using (var scope = serviceProvider.CreateScope())
                    {
                        return scope.ServiceProvider.GetRequiredService<TDbContext>().Model;
                    }
                };
            }
            //register the scalars
            Scalars.RegisterInContainer((type, instance) => { services.AddSingleton(type, instance); });
            //register the argument graphs
            ArgumentGraphs.RegisterInContainer((type, instance) => { services.AddSingleton(type, instance); });
            //register the IEfGraphQLService
            services.AddSingleton(
                typeof(IEfGraphQLService<TDbContext>),
                (serviceProvider) => new EfGraphQLService<TDbContext>(
                    dbModelCreator(serviceProvider),
                    filters == null ? new GlobalFilters() : filters(serviceProvider),
                    dbContextFromUserContext)
            );
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