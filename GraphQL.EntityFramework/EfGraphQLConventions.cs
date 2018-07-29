using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Types.Relay;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
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

            var service = new EfGraphQLService(GetNavigationProperties(dbContext));
            registerInstance(typeof(IEfGraphQLService), service);
        }

        public static void RegisterInContainer(IServiceCollection services, DbContext dbContext)
        {
            Guard.AgainstNull(nameof(services), services);
            Guard.AgainstNull(nameof(dbContext), dbContext);
            RegisterInContainer((type, instance) => { services.AddSingleton(type, instance); }, dbContext);
        }

        public static void RegisterConnectionTypesInContainer(IServiceCollection services)
        {
            Guard.AgainstNull(nameof(services), services);
            services.AddTransient(typeof(ConnectionType<>));
            services.AddTransient(typeof(EdgeType<>));
            services.AddSingleton<PageInfoType>();
        }

        static Dictionary<Type, List<Navigation>> GetNavigationProperties(DbContext dbContext)
        {
            return dbContext.Model
                .GetEntityTypes()
                .ToDictionary(x => x.ClrType, GetNavigations);
        }

        static List<Navigation> GetNavigations(IEntityType entity)
        {
            var navigations = entity.GetNavigations();
            return navigations
                .Select(
                    x => new Navigation
                    {
                        PropertyName = x.Name,
                        PropertyType = GetNavigationType(x)
                    })
                .ToList();
        }

        static Type GetNavigationType(INavigation navigation)
        {
            var navigationType = navigation.ClrType;
            var collectionType = navigationType.GetInterfaces()
                .SingleOrDefault(x => x.IsGenericType &&
                                      x.GetGenericTypeDefinition() == typeof(ICollection<>));
            if (collectionType == null)
            {
                return navigationType;
            }

            return collectionType.GetGenericArguments().Single();
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