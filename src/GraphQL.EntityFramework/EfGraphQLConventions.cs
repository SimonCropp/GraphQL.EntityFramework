using System;
using System.Collections.Generic;
using System.Reflection;
using GraphQL.Types.Relay;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.EntityFramework
{
    public static class EfGraphQLConventions
    {
        #region RegisterInContainerAction
        public static void RegisterInContainer(Action<Type, object> register, IModel model, GlobalFilters filters = null)
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

            var service = new EfGraphQLService(model, filters);
            register(typeof(IEfGraphQLService), service);
        }

        #region RegisterInContainerServiceCollection
        public static void RegisterInContainer(IServiceCollection services, IModel model, GlobalFilters filters = null)
        #endregion
        {
            Guard.AgainstNull(nameof(services), services);
            Guard.AgainstNull(nameof(model), model);
            services.AddTransient(typeof(ConnectionType<>));
            services.AddTransient(typeof(EdgeType<>));
            services.AddSingleton<PageInfoType>();
            RegisterInContainer((type, instance) => { services.AddSingleton(type, instance); }, model, filters);
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

        #region RegisterInContainerServiceCollection
        public static void RegisterInContainer(IServiceCollection services, IEnumerable<Assembly> typeConfigurationAssemblies, GlobalFilters filters = null)
        #endregion
        {
            Guard.AgainstNull(nameof(services), services);

            var builder = new DbContextOptionsBuilder();
            builder.UseSqlServer("fake");
            using (var context = new DbContext(builder.Options))
            {
                var modelBuilder = new ModelBuilder(ConventionSet.CreateConventionSet(context));
                foreach (var assembly in typeConfigurationAssemblies)
                {
                    modelBuilder.ApplyConfigurationsFromAssembly(assembly);
                }

                RegisterInContainer(services, modelBuilder.Model, filters);
            }
        }
    }
}