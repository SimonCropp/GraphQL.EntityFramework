using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework
{
    public static class Mapper<TDbContext>
        where TDbContext : DbContext
    {
        static HashSet<Type> ignoredTypes = new();

        /// <summary>
        /// Add a property type to exclude from mapping.
        /// </summary>
        public static void AddIgnoredType<T>()
        {
            ignoredTypes.Add(typeof(T));
        }

        /// <summary>
        /// Add a property type to exclude from mapping.
        /// </summary>
        public static void AddIgnoredType(Type type)
        {
            ignoredTypes.Add(type);
        }

        const BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Static;
        static MethodInfo addNavigationMethod = typeof(Mapper<TDbContext>).GetMethod(nameof(AddNavigation), bindingFlags)!;
        static MethodInfo addNavigationListMethod = typeof(Mapper<TDbContext>).GetMethod(nameof(AddNavigationList), bindingFlags)!;

        /// <summary>
        /// Map all un-mapped properties. Calls <see cref="ComplexGraphType{TSourceType}.AddField"/> for all other properties.
        /// </summary>
        /// <param name="exclusions">A list of property names to exclude from mapping.</param>
        public static void AutoMap<TSource>(
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
                throw new($"Failed to map '{graph.GetType().Name}'. {exception.Message}");
            }
        }

        internal static void AutoMap<TSource>(
            ComplexGraphType<TSource> graph,
            IEfGraphQLService<TDbContext> graphService,
            IReadOnlyList<string>? exclusions = null)
        {
            var type = typeof(TSource);
            try
            {
                if (graphService.Navigations.TryGetValue(type, out var navigations))
                {
                    MapNavigationProperties(graph, graphService, exclusions, navigations);
                }

                List<string> list = new();
                if (exclusions != null)
                {
                    list.AddRange(exclusions);
                }

                if (navigations != null)
                {
                    list.AddRange(navigations.Select(x => x.Name));
                }

                MapProperties(graph, type, list);
            }
            catch (GetGraphException exception)
            {
                throw new($"Failed to map '{graph.GetType().Name}'. {exception.Message}");
            }
        }

        static void MapProperties<TSource>(ComplexGraphType<TSource> graph, Type type, IReadOnlyList<string>? exclusions)
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

        static void MapNavigationProperties<TSource>(
            ComplexGraphType<TSource> graph,
            IEfGraphQLService<TDbContext> graphService,
            IReadOnlyList<string>? exclusions,
            IReadOnlyList<Navigation> navigations)
        {
            foreach (var navigation in navigations)
            {
                if (ShouldIgnore(graph, navigation.Name, navigation.Type, exclusions))
                {
                    continue;
                }

                ProcessNavigation(graph, graphService, navigation);
            }
        }

        static void ProcessNavigation<TSource>(
            ComplexGraphType<TSource> graph,
            IEfGraphQLService<TDbContext> graphService,
            Navigation navigation)
        {
            try
            {
                if (navigation.IsCollection)
                {
                    var genericMethod = addNavigationListMethod.MakeGenericMethod(typeof(TSource), navigation.Type);
                    genericMethod.Invoke(null, new object[] {graph, graphService, navigation});
                }
                else
                {
                    var genericMethod = addNavigationMethod.MakeGenericMethod(typeof(TSource), navigation.Type);
                    genericMethod.Invoke(null, new object[] {graph, graphService, navigation});
                }
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException!;
            }
        }

        static void AddNavigation<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            IEfGraphQLService<TDbContext> graphQlService,
            Navigation navigation)
            where TReturn : class
        {
            var graphTypeFromType = GraphTypeFromType(navigation.Name, navigation.Type, navigation.IsNullable);
            var compile = NavigationFunc<TSource, TReturn>(navigation.Name);
            graphQlService.AddNavigationField(graph, navigation.Name, compile, graphTypeFromType);
        }

        static void AddNavigationList<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            IEfGraphQLService<TDbContext> graphQlService,
            Navigation navigation)
            where TReturn : class
        {
            var graphTypeFromType = GraphTypeFromType(navigation.Name, navigation.Type, false);
            var compile = NavigationFunc<TSource, IEnumerable<TReturn>>(navigation.Name);
            graphQlService.AddNavigationListField(graph, navigation.Name, compile, graphTypeFromType);
        }
        public record NavigationKey(Type Type, string Name);

        static ConcurrentDictionary<NavigationKey, object> navigationFuncs = new();

        internal static Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn> NavigationFunc<TSource, TReturn>(string name)
        {
            NavigationKey key = new(typeof(TSource), name);

            return (Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn>) navigationFuncs.GetOrAdd(
                key,
                x => NavigationExpression<TSource, TReturn>(x.Name).Compile());
        }

        internal static Expression<Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn>> NavigationExpression<TSource, TReturn>(string name)
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
            var (compile, propertyGraphType) = Compile<TSource>(property);
            SimpleFieldResolver<TSource> resolver = new(compile);
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

            if (FieldExists(graphType, name))
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

            return false;
        }

        static (Func<TSource, object> resolver, Type graphType) Compile<TSource>(PropertyInfo member)
        {
            var func = PropertyCache<TSource>.GetProperty(member.Name).Func;
            var graphTypeFromType = GraphTypeFromType(member.Name, member.PropertyType, member.IsNullable());
            return (func, graphTypeFromType);
        }

        internal static Expression<Func<TSource, object>> PropertyToObject<TSource>(string member)
        {
            // TSource parameter
            var parameter = Expression.Parameter(typeof(TSource), "source");
            // get property from source instance
            var property = Expression.Property(parameter, member);
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

        static bool FieldExists(IComplexGraphType graphType, string name)
        {
            return graphType.Fields.Any(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        }
    }
}