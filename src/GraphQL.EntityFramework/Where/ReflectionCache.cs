static class ReflectionCache
{
    public static MethodInfo StringAny =
        typeof(Enumerable)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Single(_ => _.Name == "Any" &&
                         _.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(string));

    public static MethodInfo StringLike =
        typeof(DbFunctionsExtensions)
            .GetMethod("Like", [typeof(DbFunctions), typeof(string), typeof(string)])!;

    public static MethodInfo StringEqual =
        typeof(string)
            .GetMethod("Equals", [typeof(string), typeof(string)])!;

    public static MethodInfo StringStartsWith =
        typeof(string)
            .GetMethod("StartsWith", [typeof(string)])!;

    public static MethodInfo StringIndexOf =
        typeof(string)
            .GetMethod("IndexOf", [typeof(string)])!;

    public static MethodInfo StringEndsWith =
        typeof(string)
            .GetMethod("EndsWith", [typeof(string)])!;

    static FrozenDictionary<Type, MethodInfo> listContainsCache;

    static ReflectionCache() =>
        listContainsCache = FrozenDictionary.Create(
            new KeyValuePair<Type, MethodInfo>(typeof(Guid), GetContains<Guid>()),
            new KeyValuePair<Type, MethodInfo>(typeof(Guid?), GetContains<Guid?>()),
            new KeyValuePair<Type, MethodInfo>(typeof(bool), GetContains<bool>()),
            new KeyValuePair<Type, MethodInfo>(typeof(bool?), GetContains<bool?>()),
            new KeyValuePair<Type, MethodInfo>(typeof(int), GetContains<int>()),
            new KeyValuePair<Type, MethodInfo>(typeof(int?), GetContains<int?>()),
            new KeyValuePair<Type, MethodInfo>(typeof(short), GetContains<short>()),
            new KeyValuePair<Type, MethodInfo>(typeof(short?), GetContains<short?>()),
            new KeyValuePair<Type, MethodInfo>(typeof(long), GetContains<long>()),
            new KeyValuePair<Type, MethodInfo>(typeof(long?), GetContains<long?>()),
            new KeyValuePair<Type, MethodInfo>(typeof(uint), GetContains<uint>()),
            new KeyValuePair<Type, MethodInfo>(typeof(uint?), GetContains<uint?>()),
            new KeyValuePair<Type, MethodInfo>(typeof(ushort), GetContains<ushort>()),
            new KeyValuePair<Type, MethodInfo>(typeof(ushort?), GetContains<ushort?>()),
            new KeyValuePair<Type, MethodInfo>(typeof(ulong), GetContains<ulong>()),
            new KeyValuePair<Type, MethodInfo>(typeof(ulong?), GetContains<ulong?>()),
            new KeyValuePair<Type, MethodInfo>(typeof(DateTime), GetContains<DateTime>()),
            new KeyValuePair<Type, MethodInfo>(typeof(DateTime?), GetContains<DateTime?>()),
            new KeyValuePair<Type, MethodInfo>(typeof(DateTimeOffset), GetContains<DateTimeOffset>()),
            new KeyValuePair<Type, MethodInfo>(typeof(DateTimeOffset?), GetContains<DateTimeOffset?>()));

    public static MethodInfo? GetListContains(Type type)
    {
        if (type == typeof(string))
        {
            return null;
        }

        // Try FrozenDictionary lookup first - O(1) for common types
        if (listContainsCache.TryGetValue(type, out var method))
        {
            return method;
        }

        if (IsEnumType(type))
        {
            return typeof(ICollection<>).MakeGenericType(type).GetMethod("Contains");
        }

        return null;
    }

    static MethodInfo GetContains<T>() =>
        typeof(ICollection<T>).GetMethod("Contains")!;

    public static bool TryGetEnumType(this Type type, [NotNullWhen(true)] out Type? enumType)
    {
        if (type.IsEnum)
        {
            enumType = type;
            return true;
        }

        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying is null)
        {
            enumType = typeof(object);
            return false;
        }

        if (underlying.IsEnum)
        {
            enumType = underlying;
            return true;
        }

        enumType = null;
        return false;
    }

    public static bool IsEnumType(this Type type)
    {
        if (type.IsEnum)
        {
            return true;
        }

        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying is null)
        {
            return false;
        }

        if (underlying.IsEnum)
        {
            return true;
        }

        return false;
    }

    public static bool TryGetCollectionType(this Type type, [NotNullWhen(true)] out Type? collectionGenericType)
    {
        Type? collectionType;

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>))
        {
            collectionType = type;
        }
        else
        {
            collectionType = type.GetInterfaces()
                .SingleOrDefault(_ => _.IsGenericType &&
                                      _.GetGenericTypeDefinition() == typeof(ICollection<>));
        }

        if (collectionType is null)
        {
            collectionGenericType = null;
            return false;
        }

        collectionGenericType = collectionType.GetGenericArguments().Single();
        return true;
    }

    public static IEnumerable<PropertyInfo> GetPublicProperties(this Type type)
    {
        const BindingFlags flags = BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance;

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
                .Where(_ => !propertyInfos.Contains(_));

            propertyInfos.AddRange(newPropertyInfos);
        }

        return propertyInfos;
    }
}
