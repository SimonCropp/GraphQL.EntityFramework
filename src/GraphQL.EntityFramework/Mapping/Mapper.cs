using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework
{
    public static class Mapper
    {
        static HashSet<Type> ignoredTypes = new HashSet<Type>();

        public static void AddIgnoredType<T>()
        {
            ignoredTypes.Add(typeof(T));
        }

        public static void AddIgnoredType(Type type)
        {
            ignoredTypes.Add(type);
        }

        const BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Static;
        static MethodInfo addNavigationMethod = typeof(Mapper).GetMethod(nameof(AddNavigation), bindingFlags);
        static MethodInfo addNavigationListMethod = typeof(Mapper).GetMethod(nameof(AddNavigationList), bindingFlags);

        public static void AutoMap<TSource>(this
            ComplexGraphType<TSource> graph,
            IReadOnlyList<string>? exclusions = null)
        {
            var type = typeof(TSource);
            try
            {
                MapProperties(graph, type, exclusions);
            }
            catch (GetGraphException exception)
            {
                throw new Exception($"Failed to map '{graph.GetType().Name}'. {exception.Message}");
            }
        }

        internal static void AutoMap<TDbContext, TSource>(
            ComplexGraphType<TSource> graph,
            IEfGraphQLService<TDbContext> graphService,
            IReadOnlyList<string>? exclusions = null)
            where TDbContext : DbContext
        {
            var type = typeof(TSource);
            try
            {
                MapNavigationProperties(graph, graphService, type, exclusions);

                MapProperties(graph, type, exclusions);
            }
            catch (GetGraphException exception)
            {
                throw new Exception($"Failed to map '{graph.GetType().Name}'. {exception.Message}");
            }
        }

        static void MapProperties<TSource>(ComplexGraphType<TSource> graph, Type type, IReadOnlyList<string>? exclusions = null)
        {
            var publicProperties = type.GetPublicProperties()
                .OrderBy(x => x.Name);
            foreach (var property in publicProperties)
            {
                if (ShouldIgnore(graph, property.Name, property.PropertyType, exclusions))
                {
                    continue;
                }

                AddMember(graph, property);
            }
        }

        static void MapNavigationProperties<TDbContext, TSource>(
            ComplexGraphType<TSource> graph,
            IEfGraphQLService<TDbContext> graphService,
            Type type,
            IReadOnlyList<string>? exclusions = null)
            where TDbContext : DbContext
        {
            if (!graphService.Navigations.TryGetValue(type, out var navigations))
            {
                return;
            }

            foreach (var navigation in navigations)
            {
                if (ShouldIgnore(graph, navigation.Name, navigation.Type, exclusions))
                {
                    continue;
                }

                ProcessNavigation(graph, graphService, navigation);
            }
        }

        static void ProcessNavigation<TDbContext, TSource>(
            ComplexGraphType<TSource> graph,
            IEfGraphQLService<TDbContext> graphService,
            Navigation navigation)
            where TDbContext : DbContext
        {
            try
            {
                if (navigation.IsCollection)
                {
                    var genericMethod = addNavigationListMethod.MakeGenericMethod(typeof(TDbContext), typeof(TSource), navigation.Type);
                    genericMethod.Invoke(null, new object[] {graph, graphService, navigation});
                }
                else
                {
                    var genericMethod = addNavigationMethod.MakeGenericMethod(typeof(TDbContext), typeof(TSource), navigation.Type);
                    genericMethod.Invoke(null, new object[] {graph, graphService, navigation});
                }
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException;
            }
        }

        static void AddNavigation<TDbContext, TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            IEfGraphQLService<TDbContext> graphQlService,
            Navigation navigation)
            where TDbContext : DbContext
            where TReturn : class
        {
            var graphTypeFromType = GraphTypeFromType(navigation.Name, navigation.Type, navigation.IsNullable);
            var compile = NavigationExpression<TDbContext, TSource, TReturn>(navigation.Name).Compile();
            graphQlService.AddNavigationField(graph, navigation.Name, compile, graphTypeFromType);
        }

        static void AddNavigationList<TDbContext, TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            IEfGraphQLService<TDbContext> graphQlService,
            Navigation navigation)
            where TDbContext : DbContext
            where TReturn : class
        {
            var graphTypeFromType = GraphTypeFromType(navigation.Name, navigation.Type, false);
            var compile = NavigationExpression<TDbContext, TSource, IEnumerable<TReturn>>(navigation.Name).Compile();
            graphQlService.AddNavigationListField(graph, navigation.Name, compile, graphTypeFromType);
        }

        internal static Expression<Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn>> NavigationExpression<TDbContext, TSource, TReturn>(string name)
            where TDbContext : DbContext
        {
            // TSource parameter
            var type = typeof(ResolveEfFieldContext<TDbContext, TSource>);
            var parameter = Expression.Parameter(type, "context");
            var sourcePropertyInfo = type.GetProperty("Source", typeof(TSource))!;
            var sourceProperty = Expression.Property(parameter, sourcePropertyInfo);
            var property = Expression.Property(sourceProperty, name);

            //context => context.Source.Parent
            return Expression.Lambda<Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn>>(property, parameter);
        }

        static void AddMember<TSource>(ComplexGraphType<TSource> graph, PropertyInfo property)
        {
            if (ShouldIgnore(graph, property.Name, property.PropertyType))
            {
                return;
            }

            var (compile, propertyGraphType) = Compile<TSource>(property);
            var resolver = new SimpleFieldResolver<TSource>(compile);
            var graphQlField = graph.Field(type: propertyGraphType, name: property.Name);
            graphQlField.Resolver = resolver;
        }

        static bool ShouldIgnore(IComplexGraphType graphType, string name, Type propertyType, IReadOnlyList<string>? localIgnores = null)
        {
            if (localIgnores != null)
            {
                if (localIgnores.Contains(name, StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            if (graphType.FieldExists(name))
            {
                return true;
            }

            if (propertyType == typeof(string))
            {
                return false;
            }

            if (ignoredTypes.Contains(propertyType))
            {
                return true;
            }

            if (propertyType.TryGetCollectionType(out var collectionType))
            {
                var itemType = collectionType.GenericTypeArguments.Single();
                if (ignoredTypes.Contains(itemType))
                {
                    return true;
                }

                return itemType.IsClass;
            }

            return false;
        }

        static (Func<TSource, object> resolver, Type graphType) Compile<TSource>(PropertyInfo member)
        {
            var lambda = PropertyToObject<TSource>(member);
            var graphTypeFromType = GraphTypeFromType(member.Name, member.PropertyType, member.IsNullable());
            return (lambda.Compile(), graphTypeFromType);
        }

        internal static Expression<Func<TSource, object>> PropertyToObject<TSource>(PropertyInfo member)
        {
            // TSource parameter
            var parameter = Expression.Parameter(typeof(TSource), "source");
            // get property from source instance
            var property = Expression.Property(parameter, member.Name);
            // convert member instance to object
            var convert = Expression.Convert(property, typeof(object));

            return Expression.Lambda<Func<TSource, object>>(convert, parameter);
        }

        static Type GraphTypeFromType(string name, Type propertyType, bool isNullable)
        {
            try
            {
                return propertyType.GetGraphTypeFromType(isNullable);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                var message = $"Unable to get graph for '{name}'. To exclude use the `exclusions` parameter when calling `AutoMap`. Error {exception.Message}";
                throw new GetGraphException(message);
            }
        }

        static bool FieldExists(this IComplexGraphType graphType, string name)
        {
            return graphType.Fields.Any(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        }
    }
}