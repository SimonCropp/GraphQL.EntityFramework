﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

static class PropertyCache<TInput>
{
    public static ParameterExpression SourceParameter = Expression.Parameter(typeof(TInput));
    static ConcurrentDictionary<string, Property<TInput>> properties = new();

    public static Property<TInput> GetProperty(string path)
    {
        return properties.GetOrAdd(
            path,
            x =>
            {
                var left = AggregatePath(x, SourceParameter);

                var converted = Expression.Convert(left, typeof(object));
                var lambda = Expression.Lambda<Func<TInput, object>>(converted, SourceParameter);
                var compile = lambda.Compile();
                var listContainsMethod = ReflectionCache.GetListContains(left.Type);
                
                var body = (MemberExpression)left;
                return new Property<TInput>
                (
                    Left: left,
                    Lambda: lambda,
                    SourceParameter: SourceParameter,
                    Func: compile,
                    PropertyType: left.Type,
                    Info: body.Member,
                    ListContainsMethod: listContainsMethod
                );
            });
    }

    static Expression AggregatePath(string path, Expression parameter)
    {
        try
        {
            return path.Split('.')
                .Aggregate(parameter, (current, property) =>
                    Expression.MakeMemberAccess(current, GetPropertyOrField(current.Type, property)));
        }
        catch (ArgumentException exception)
        {
            throw new($"Failed to create a member expression. Type: {typeof(TInput).FullName}, Path: {path}. Error: {exception.Message}");
        }
    }

    /// <summary>
    /// Get Specified property or field member info of provided type
    /// </summary>
    /// <param name="type">Type to retrieve property from</param>
    /// <param name="propertyOrFieldName">Name of property or field</param>
    static MemberInfo GetPropertyOrField(Type type, string propertyOrFieldName)
    {
        // Member search binding flags
        const BindingFlags bindingFlagsPublic = BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy;
        const BindingFlags bindingFlagsNonPublic = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy;

        // Attempt to get the public property
        MemberInfo? propertyOrField = type.GetProperty(propertyOrFieldName, bindingFlagsPublic);

        // If not found
        if (propertyOrField is null)
        {
            // Attempt to get public field
            propertyOrField = type.GetField(propertyOrFieldName, bindingFlagsPublic);
        }

        // If not found
        if (propertyOrField is null)
        {
            // Attempt to get non-public property
            propertyOrField = type.GetProperty(propertyOrFieldName, bindingFlagsNonPublic);
        }

        // If not found
        if (propertyOrField is null)
        {
            // Attempt to get non-public property
            propertyOrField = type.GetField(propertyOrFieldName, bindingFlagsNonPublic);
        }

        // If property/ field was not resolved
        if (propertyOrField == null && type.IsInterface)
        {
            // Get All the implemented interfaces of the type
            List<Type> baseInterfaces = new(type.GetInterfaces());

            // Iterate over inherited interfaces
            foreach (var baseInterfaceType in baseInterfaces)
            {
                // Recurse looking in the parent interface for the property
                propertyOrField = GetPropertyOrField(baseInterfaceType, propertyOrFieldName);

                // If property found
                if (propertyOrField is not null)
                {
                    // Break execution
                    break;
                }
            }
        }

        // If property is still empty
        if (propertyOrField is null)
        {
            // Property does not exist on current type
            throw new ArgumentException($"'{propertyOrFieldName}' is not a member of type {type.FullName}");
        }

        return propertyOrField;
    }
}