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

    public static void AutoMap<TDbContext,TSource>(ObjectGraphType<TSource> graph, IEfGraphQLService<TDbContext> graphQlService, IEnumerable<string>? exclusions = null)
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
        if(graphQlService.Navigations.TryGetValue(type, out var navigations))
        {
            foreach (var navigation in navigations)
            {
                AddNavigationField(graph, graphQlService, navigation);
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

    private static void AddNavigationField<TDbContext, TSource>(
        ObjectGraphType<TSource> graph,
        IEfGraphQLService<TDbContext> graphQlService,
        Navigation navigation)
        where TDbContext : DbContext
    {
        //TODO: detect nullable
        var graphTypeFromType = navigation.PropertyType.GetGraphTypeFromType();
        Func<ResolveEfFieldContext<TDbContext, TSource>, object?> compile = NavigationExpression<TDbContext,TSource>(navigation.PropertyName).Compile();
        graphQlService.AddNavigationField(graph, navigation.PropertyName, compile, graphTypeFromType);
    }


    internal static Expression<Func<ResolveEfFieldContext<TDbContext, TSource>, object?>> NavigationExpression<TDbContext, TSource>(string name)
        where TDbContext : DbContext
    {
        // TSource parameter
        var parameter = Expression.Parameter(typeof(ResolveEfFieldContext<TDbContext, TSource>), "context");

        var sourceProperty = Expression.Property(parameter, "Source");
        var property = Expression.Property(sourceProperty, name);
        var convert = Expression.Convert(property, typeof(object));

        //context => context.Source.Parent
        return Expression.Lambda<Func<ResolveEfFieldContext<TDbContext, TSource>, object?>>(convert, parameter);
    }

    static void AddMember<TSource>(ComplexGraphType<TSource> graphType, PropertyInfo property)
    {
        if (graphType.FieldExists(property.Name))
        {
            return;
        }

        if (ShouldIgnore(property.PropertyType))
        {
            return;
        }

        var (compile, propertyGraphType) = Compile<TSource>(property);
        var resolver = new SimpleFieldResolver<TSource>(compile);
        var graphQlField = graphType.Field(type: propertyGraphType, name: property.Name);
        graphQlField.Resolver = resolver;
    }

    static (Func<TSource, object> resolver, Type graphType) Compile<TSource>(PropertyInfo member)
    {
        var lambda = PropertyToObject<TSource>(member);
        var graphTypeFromType = GetGraphType(member);
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

    static Type GetGraphType(PropertyInfo member)
    {
        var propertyType = member.PropertyType;
        try
        {
            return propertyType.GetGraphTypeFromType(member.IsNullable());
        }
        catch (Exception exception)
        {
            throw new Exception($"Unable to map {member.Name} on {member.DeclaringType!.FullName}. Error {exception.Message}", exception);
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

    static bool FieldExists<TSource>(this ComplexGraphType<TSource> graphType, string name)
    {
        return graphType.Fields.Any(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
    }
}