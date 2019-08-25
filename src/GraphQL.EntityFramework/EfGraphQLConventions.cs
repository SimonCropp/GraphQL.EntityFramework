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
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <param name="resolveDbContext">A function to obtain the <typeparamref name="TDbContext"/> from the GraphQL user context.</param>
        /// <param name="model">The <see cref="IModel"/> to use. If null, then it will be extracted from the <see cref="IServiceProvider"/>.</param>
        /// <param name="resolveFilters">A function to obtain a list of filters to apply to the returned data.</param>
        #region RegisterInContainer
        public static void RegisterInContainer<TDbContext>(
                IServiceCollection services,
                ResolveDbContext<TDbContext> resolveDbContext = null,
                IModel model = null,
                ResolveFilters resolveFilters = null)
            #endregion
            where TDbContext : DbContext
        {
            Guard.AgainstNull(nameof(services), services);

            RegisterScalarsAndArgs(services);

            services.AddSingleton<IEfGraphQLService<TDbContext>>(
                serviceProvider =>
                {
                    model = model ?? ResolveModel<TDbContext>(serviceProvider);

                    resolveFilters = resolveFilters ?? serviceProvider.GetService<ResolveFilters>();

                    if (resolveDbContext == null)
                    {
                        resolveDbContext = context => serviceProvider.GetRequiredService<TDbContext>();
                    }

                    return new EfGraphQLService<TDbContext>(
                        model,
                        resolveDbContext,
                        resolveFilters);
                });
        }

        ///// <summary>
        ///// Register the necessary services with the service provider for a data context of <typeparamref name="TDbContext"/>
        ///// </summary>
        ///// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        ///// <param name="resolveDbContext">A function to obtain the <typeparamref name="TDbContext"/> from the GraphQL user context.</param>
        ///// <param name="model">The <see cref="IModel"/> for <typeparamref name="TDbContext"/>.</param>
        ///// <param name="resolveFilters">The <see cref="Filters"/> to use.</param>
        //public static void RegisterInContainer<TDbContext>(
        //    IServiceCollection services,
        //    ResolveDbContext<TDbContext> resolveDbContext,
        //    IModel model,
        //    ResolveFilters resolveFilters = null)
        //    where TDbContext : DbContext
        //{
        //    Guard.AgainstNull(nameof(services), services);
        //    Guard.AgainstNull(nameof(model), model);

        //    RegisterScalarsAndArgs(services);

        //    services.AddSingleton<IEfGraphQLService<TDbContext>>(
        //        serviceProvider => new EfGraphQLService<TDbContext>(
        //            model,
        //            resolveDbContext,
        //            resolveFilters)
        //    );
        //}

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

        static IModel ResolveModel<TDbContext>(IServiceProvider serviceProvider)
            where TDbContext : DbContext
        {
            var model = serviceProvider.GetService<IModel>();
            if (model != null)
            {
                return model;
            }
            var dbContext = serviceProvider.GetService<TDbContext>();
            if (dbContext != null)
            {
                return dbContext.Model;
            }
            throw new Exception($"Could not resolve {nameof(IModel)} from the {nameof(IServiceProvider)}. Tried to extract both {nameof(IModel)} and {typeof(TDbContext)}.");
        }
    }
}