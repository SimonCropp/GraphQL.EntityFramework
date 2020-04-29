using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GraphQL;
using GraphQL.EntityFramework;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

static class Mapper
{
    static HashSet<Type> ignoredTypes = new HashSet<Type>
    {
        typeof(byte[])
    };

    public static void AddIgnoredType<T>()
    {
        ignoredTypes.Add(typeof(T));
    }

    static MethodInfo addNavigationMethod = typeof(Mapper).GetMethod(nameof(AddNavigation), BindingFlags.NonPublic | BindingFlags.Static);
    static MethodInfo addNavigationListMethod = typeof(Mapper).GetMethod(nameof(AddNavigationList), BindingFlags.NonPublic | BindingFlags.Static);

    public static void AutoMap<TDbContext, TSource>(ObjectGraphType<TSource> graph, IEfGraphQLService<TDbContext> graphQlService, IEnumerable<string>? exclusions = null)
        where TDbContext : DbContext
    {
        Func<string, bool> exclude;
        if (exclusions == null)
        {
            exclude = name => false;
        }
        else
        {
            exclusions = exclusions.ToList();
            exclude = name => exclusions.Contains(name, StringComparer.OrdinalIgnoreCase);
        }

        var type = typeof(TSource);
        try
        {

            if (graphQlService.Navigations.TryGetValue(type, out var navigations))
            {
                foreach (var navigation in navigations)
                {
                    ProcessNavigation(graph, graphQlService, navigation);
                }
            }

            var publicProperties = type.GetPublicProperties()
                .OrderBy(x => x.Name);
            foreach (var property in publicProperties)
            {
                if (exclude(property.Name))
                {
                    continue;
                }

                AddMember(graph, property);
            }
        }
        catch (GetGraphException exception)
        {
            throw new Exception($"Failed to map {type.Name}. {exception.Message}");
        }
    }

    static void ProcessNavigation<TDbContext, TSource>(ObjectGraphType<TSource> graph, IEfGraphQLService<TDbContext> graphQlService, Navigation navigation)
        where TDbContext : DbContext
    {
        if (ShouldIgnore(graph, navigation.Name, navigation.Type))
        {
            return;
        }

        try
        {
            if (navigation.IsCollection)
            {
                var genericMethod = addNavigationListMethod.MakeGenericMethod(typeof(TDbContext), typeof(TSource), navigation.Type);
                genericMethod.Invoke(null, new object[] {graph, graphQlService, navigation});
            }
            else
            {
                var genericMethod = addNavigationMethod.MakeGenericMethod(typeof(TDbContext), typeof(TSource), navigation.Type);
                genericMethod.Invoke(null, new object[] {graph, graphQlService, navigation});
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
        var graphTypeFromType = GraphTypeFromType(navigation.Name, navigation.Type,  navigation.IsNullable);
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
        var graphTypeFromType = GraphTypeFromType(navigation.Name, navigation.Type,  navigation.IsNullable);
        var compile = NavigationExpression<TDbContext, TSource, IEnumerable<TReturn>>(navigation.Name).Compile();
        graphQlService.AddNavigationListField(graph, navigation.Name, compile, graphTypeFromType);
    }

    internal static Expression<Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn>> NavigationExpression<TDbContext, TSource, TReturn>(string name)
        where TDbContext : DbContext
    {
        // TSource parameter
        var parameter = Expression.Parameter(typeof(ResolveEfFieldContext<TDbContext, TSource>), "context");

        var sourceProperty = Expression.Property(parameter, "Source");
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

    static bool ShouldIgnore(IComplexGraphType graphType, string propertyName, Type propertyType)
    {
        if (graphType.FieldExists(propertyName))
        {
            return true;
        }

        if (ShouldIgnore(propertyType))
        {
            return true;
        }

        return false;
    }

    static (Func<TSource, object> resolver, Type graphType) Compile<TSource>(PropertyInfo member)
    {
        var lambda = PropertyToObject<TSource>(member);
        var graphTypeFromType = GraphTypeFromType(member.Name, member.PropertyType,  member.IsNullable());
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
            var message = $"Unable to GetGraphTypeFromType for {name}. To exclude use the `exclusions` parameter when calling `AutoMap`. Error {exception.Message}";
            throw new GetGraphException(message);
        }
    }

    static bool ShouldIgnore(Type memberType)
    {
        if (ignoredTypes.Contains(memberType))
        {
            return true;
        }

        if (memberType == typeof(string))
        {
            return false;
        }

        if (memberType.TryGetCollectionType(out var collectionType))
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

    static bool FieldExists(this IComplexGraphType graphType, string name)
    {
        return graphType.Fields.Any(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
    }
}