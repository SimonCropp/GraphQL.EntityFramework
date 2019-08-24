using System;
using GraphQL.Types.Relay;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.EntityFramework
{
    public static class EfGraphQLConventions
    {
        /// <summary>
        /// Register the necessary services with the service provider for a data context of <typeparamref name="TDbContext"/>
        /// </summary>
        /// <typeparam name="TDbContext"></typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <param name="dbContextFromUserContext">A function to obtain the <typeparamref name="TDbContext"/> from the GraphQL user context.</param>
        /// <param name="filters">A function to obtain a list of filters to apply to the returned data.</param>
        #region RegisterInContainerViaServiceProvider
        public static void RegisterInContainer<TDbContext>(
            IServiceCollection services,
            DbContextFromUserContext<TDbContext> dbContextFromUserContext,
            Func<IServiceProvider, GlobalFilters> filters = null)
            where TDbContext : DbContext
        #endregion
        {
            Guard.AgainstNull(nameof(services), services);
            GlobalFilters Filters(IServiceProvider serviceProvider)
            {
                if (filters == null)
                {
                    return new GlobalFilters();
                }

                return filters(serviceProvider) ?? new GlobalFilters();
            }


            RegisterScalarsAndArgs(services);
            //register the IEfGraphQLService
            services.AddSingleton(
                typeof(IEfGraphQLService<TDbContext>),
                serviceProvider => new EfGraphQLService<TDbContext>(
                    //acquire the database model via the service provider
                    serviceProvider.GetRequiredService<TDbContext>().Model,
                    Filters(serviceProvider),
                    dbContextFromUserContext)
            );
        }

        static void RegisterScalarsAndArgs(IServiceCollection services)
        {
            //register the scalars
            Scalars.RegisterInContainer(services);
            //register the argument graphs
            ArgumentGraphs.RegisterInContainer(services);
        }

        public static void RegisterConnectionTypesInContainer(IServiceCollection services)
        {
            Guard.AgainstNull(nameof(services), services);
            services.AddTransient(typeof(ConnectionType<>));
            services.AddTransient(typeof(EdgeType<>));
            services.AddSingleton<PageInfoType>();
        }
    }
}