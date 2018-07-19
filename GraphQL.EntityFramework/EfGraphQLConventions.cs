using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Types.Relay;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.EntityFramework
{
    public static class EfGraphQLConventions
    {
        public static void RegisterInContainer(Action<Type, object> registerInstance, DbContext dbContext)
        {
            Scalars.RegisterInContainer(registerInstance);
            ArgumentGraphs.RegisterInContainer(registerInstance);

            var service = new EfGraphQLService(GetNavigationProperties(dbContext));
            registerInstance(typeof(EfGraphQLService), service);
        }

        public static void RegisterInContainer(IServiceCollection services, DbContext dbContext)
        {
            RegisterInContainer((type, instance) => { services.AddSingleton(type, instance); }, dbContext);
        }

        public static void RegisterConnectionTypesInContainer(IServiceCollection services)
        {
            services.AddTransient(typeof(ConnectionType<>));
            services.AddTransient(typeof(EdgeType<>));
            services.AddSingleton<PageInfoType>();
        }

        static Dictionary<Type, List<string>> GetNavigationProperties(DbContext dbContext)
        {
            var dictionary = new Dictionary<Type, List<string>>();
            foreach (var entity in dbContext.Model.GetEntityTypes())
            {
                dictionary.Add(entity.ClrType, entity.GetNavigations().Select(x => x.Name).ToList());
            }
            return dictionary;
        }

        public static void RegisterConnectionTypesInContainer(Action<Type> register)
        {
            register(typeof(ConnectionType<>));
            register(typeof(EdgeType<>));
            register(typeof(PageInfoType));
        }
    }
}