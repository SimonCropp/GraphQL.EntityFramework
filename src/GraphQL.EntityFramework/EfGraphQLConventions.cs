using System;
using GraphQL.Types.Relay;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
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
        /// <param name="resolveDbContext">A function to obtain the <typeparamref name="TDbContext"/> from the GraphQL user context.</param>
        /// <param name="filters">A function to obtain a list of filters to apply to the returned data.</param>
        #region RegisterInContainerViaServiceProvider
        public static void RegisterInContainer<TDbContext>(
                IServiceCollection services,
                ResolveDbContext<TDbContext> resolveDbContext,
                Func<IServiceProvider, GlobalFilters> filters = null)
            #endregion
            where TDbContext : DbContext
        {
            Guard.AgainstNull(nameof(services), services);

            GlobalFilters Filters(IServiceProvider serviceProvider)
            {
                if (filters == null)
                {
                    return new GlobalFilters();
                }

                return filters(serviceProvider) ?? GlobalFilters.Empty;
            }

            RegisterScalarsAndArgs(services);

            //register the IEfGraphQLService
            services.AddSingleton(
                typeof(IEfGraphQLService<TDbContext>),
                serviceProvider => new EfGraphQLService<TDbContext>(
                    //acquire the database model via the service provider
                    serviceProvider.GetRequiredService<TDbContext>().Model,
                    Filters(serviceProvider),
                    resolveDbContext)
            );
        }

        /// <summary>
        /// Register the necessary services with the service provider for a data context of <typeparamref name="TDbContext"/>
        /// </summary>
        /// <typeparam name="TDbContext"></typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <param name="resolveDbContext">A function to obtain the <typeparamref name="TDbContext"/> from the GraphQL user context.</param>
        /// <param name="model">The <see cref="IModel"/> for <typeparamref name="TDbContext"/>.</param>
        /// <param name="extractFilterFromServices">true to extract filters from the <see cref="IServiceProvider"/>; false to use no filters.</param>
        #region RegisterInContainer
        public static void RegisterInContainer<TDbContext>(
                IServiceCollection services,
                ResolveDbContext<TDbContext> resolveDbContext,
                IModel model,
                bool extractFilterFromServices = false)
            #endregion
            where TDbContext : DbContext
        {
            Guard.AgainstNull(nameof(services), services);
            Guard.AgainstNull(nameof(model), model);

            RegisterScalarsAndArgs(services);

            if (extractFilterFromServices)
            {
                services.AddSingleton<IEfGraphQLService<TDbContext>>(
                    serviceProvider => new EfGraphQLService<TDbContext>(
                        model,
                        serviceProvider.GetRequiredService<GlobalFilters>(),
                        resolveDbContext)
                );
                return;
            }

            //register the IEfGraphQLService
            services.AddSingleton<IEfGraphQLService<TDbContext>>(
                serviceProvider => new EfGraphQLService<TDbContext>(
                    model,
                    GlobalFilters.Empty,
                    resolveDbContext)
            );
        }

        /// <summary>
        /// Register the necessary services with the service provider for a data context of <typeparamref name="TDbContext"/>
        /// </summary>
        /// <typeparam name="TDbContext"></typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <param name="resolveDbContext">A function to obtain the <typeparamref name="TDbContext"/> from the GraphQL user context.</param>
        /// <param name="model">The <see cref="IModel"/> for <typeparamref name="TDbContext"/>.</param>
        /// <param name="filters">The <see cref="GlobalFilters"/> to use.</param>
        public static void RegisterInContainer<TDbContext>(
            IServiceCollection services,
            ResolveDbContext<TDbContext> resolveDbContext,
            IModel model,
            GlobalFilters filters)
            where TDbContext : DbContext
        {
            Guard.AgainstNull(nameof(services), services);
            Guard.AgainstNull(nameof(model), model);
            Guard.AgainstNull(nameof(filters), filters);

            RegisterScalarsAndArgs(services);

            //register the IEfGraphQLService
            services.AddSingleton<IEfGraphQLService<TDbContext>>(
                serviceProvider => new EfGraphQLService<TDbContext>(
                    model,
                    filters,
                    resolveDbContext)
            );
        }

        static void RegisterScalarsAndArgs(IServiceCollection services)
        {
            Scalars.RegisterInContainer(services);
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