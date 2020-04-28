using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public static class Mapper
    {
        static HashSet<Type> ignoredTypes = new HashSet<Type>
        {
            typeof(byte[])
        };

        public static void AddIgnoredType<T>()
        {
            ignoredTypes.Add(typeof(T));
        }

        public static void AutoMap<TSource>(ObjectGraphType<TSource> graphType, IEnumerable<string>? exclusions = null)
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
            var publicProperties = GetPublicProperties(typeof(TSource));
            foreach (var property in publicProperties)
            {
                if (exclude(property.Name))
                {
                    continue;
                }

                AddMember(graphType, property);
            }
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

            var (compile, propertyGraphType) = Compile(property);
            var resolver = new SimpleFieldResolver(compile);
            var graphQlField = graphType.Field(type: propertyGraphType, name: property.Name);
            graphQlField.Resolver = resolver;
        }

        static (Func<object, object> resolver, Type graphType) Compile(PropertyInfo member)
        {
            // parameter object
            var parameter = Expression.Parameter(typeof(object));
            // cast object to source instance
            var convertedSource = Expression.Convert(parameter, member.DeclaringType);
            // get member from source instance
            var memberExpression = Expression.PropertyOrField(convertedSource, member.Name);
            // convert member instance to object
            var convertedMember = Expression.Convert(memberExpression, typeof(object));

            var lambda = Expression.Lambda<Func<object, object>>(convertedMember, parameter);
            var graphTypeFromType = GetGraphType(member);
            return (lambda.Compile(), graphTypeFromType);
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

            var collectionType = memberType.GetInterfaces()
                .FirstOrDefault(x =>
                {
                    if (!x.IsGenericType)
                    {
                        return false;
                    }

                    var genericTypeDefinition = x.GetGenericTypeDefinition();
                    return genericTypeDefinition == typeof(ICollection<>) ||
                           genericTypeDefinition == typeof(IReadOnlyCollection<>);
                });
            if (collectionType != null)
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

        static PropertyInfo[] GetPublicProperties(this Type type)
        {
            var flags = BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance;
            if (!type.IsInterface)
            {
                return type.GetProperties(flags);
            }

            var propertyInfos = new List<PropertyInfo>();

            var considered = new List<Type>();
            var queue = new Queue<Type>();
            considered.Add(type);
            queue.Enqueue(type);
            while (queue.Count > 0)
            {
                var subType = queue.Dequeue();
                foreach (var subInterface in subType.GetInterfaces())
                {
                    if (considered.Contains(subInterface))
                    {
                        continue;
                    }

                    considered.Add(subInterface);
                    queue.Enqueue(subInterface);
                }

                var typeProperties = subType.GetProperties(flags);

                var newPropertyInfos = typeProperties
                    .Where(x => !propertyInfos.Contains(x));

                propertyInfos.InsertRange(0, newPropertyInfos);
            }

            return propertyInfos.ToArray();
        }
    }
}