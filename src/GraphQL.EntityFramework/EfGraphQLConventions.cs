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
        /// <param name="resolveDbContext">A function to obtain the <typeparamref name="TDbContext"/> from the GraphQL user context. If null, then it will be extracted from the <see cref="IServiceProvider"/>.</param>
        /// <param name="model">The <see cref="IModel"/> to use. If null, then it will be extracted from the <see cref="IServiceProvider"/>.</param>
        /// <param name="resolveFilters">A function to obtain a list of filters to apply to the returned data. If null, then it will be extracted from the <see cref="IServiceProvider"/>.</param>
        #region RegisterInContainer
        public static void RegisterInContainer<TDbContext>(
                IServiceCollection services,
                ResolveDbContext<TDbContext>? resolveDbContext = null,
                IModel? model = null,
                ResolveFilters? resolveFilters = null)
            #endregion
            where TDbContext : DbContext
        {
            Guard.AgainstNull(nameof(services), services);

            RegisterScalarsAndArgs(services);

            services.AddSingleton(
                provider => Build(resolveDbContext, model, resolveFilters, provider));
        }

        static IEfGraphQLService<TDbContext> Build<TDbContext>(
            ResolveDbContext<TDbContext>? dbContext,
            IModel? model,
            ResolveFilters? filters,
            IServiceProvider provider)
            where TDbContext : DbContext
        {
            model ??= ResolveModel<TDbContext>(provider);

            filters ??= provider.GetService<ResolveFilters>();

            if (dbContext == null)
            {
                dbContext = context => provider.GetRequiredService<TDbContext>();
            }

            return new EfGraphQLService<TDbContext>(
                model,
                dbContext,
                filters);
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

        static IModel ResolveModel<TDbContext>(IServiceProvider provider)
            where TDbContext : DbContext
        {
            var model = provider.GetService<IModel>();
            if (model != null)
            {
                return model;
            }
            var dbContext = provider.GetService<TDbContext>();
            if (dbContext != null)
            {
                return dbContext.Model;
            }
            throw new Exception($"Could not resolve {nameof(IModel)} from the {nameof(IServiceProvider)}. Tried to extract both {nameof(IModel)} and {typeof(TDbContext)}.");
        }
    }
}