namespace GraphQL.EntityFramework;

public static partial class ExpressionBuilder<T>
{
    /// <summary>
    /// Build a predicate for a supplied list of where's (Grouped or not)
    /// </summary>
    public static Expression<Func<T, bool>> BuildPredicate(IReadOnlyCollection<WhereExpression> wheres)
    {
        var expressionBody = MakePredicateBody(wheres);
        var param = PropertyCache<T>.SourceParameter;

        return Expression.Lambda<Func<T, bool>>(expressionBody, param);
    }

    static Expression MakePredicateBody(IReadOnlyCollection<WhereExpression> wheres)
    {
        Expression? mainExpression = null;
        // The connector on an expression joins it to the next one
        var previousConnector = Connector.And;

        // Iterate over wheres
        foreach (var where in wheres)
        {
            Expression nextExpression;

            // A collection node: any, all or none of the items match the grouped expressions.
            // Checked first since it carries grouped expressions of its own.
            if (where.Quantifier is { } quantifier)
            {
                nextExpression = ProcessCollection(where.Path, quantifier, where.GroupedExpressions ?? []);
                if (where.Negate)
                {
                    nextExpression = NegateExpression(nextExpression);
                }
            }
            // If there are grouped expressions
            else if (where.GroupedExpressions?.Length > 0)
            {
                // Recurse with new set of expression
                nextExpression = MakePredicateBody(where.GroupedExpressions);

                // If the whole group is to be negated
                if (where.Negate)
                {
                    // Negate it
                    nextExpression = NegateExpression(nextExpression);
                }
            }
            // Otherwise handle single expressions
            else
            {
                // Get the predicate body for the single expression
                nextExpression = MakePredicateBody(where.Path, where.Comparison, where.Value, where.Negate);
            }

            // If this is the first where processed
            if (mainExpression is null)
            {
                // Assign to main expression
                mainExpression = nextExpression;
            }
            else
            {
                // Otherwise combine expression by specified connector or default (AND) if not provided
                mainExpression = CombineExpressions(previousConnector, mainExpression, nextExpression);
            }

            previousConnector = where.Connector;
        }

        return mainExpression ?? Expression.Constant(false);
    }

    /// <summary>
    /// Create a single predicate for the single set of supplied conditional arguments
    /// </summary>
    public static Expression<Func<T, bool>> BuildPredicate(string path, Comparison comparison, string?[]? values, bool negate = false)
    {
        var expressionBody = MakePredicateBody(path, comparison, values, negate);
        var param = PropertyCache<T>.SourceParameter;

        return Expression.Lambda<Func<T, bool>>(expressionBody, param);
    }

    /// <summary>
    /// A predicate for a single expression, or a tree of them.
    /// </summary>
    public static Expression<Func<T, bool>> BuildPredicate(WhereExpression where) =>
        BuildPredicate([where]);

    static Expression MakePredicateBody(string path, Comparison comparison, object?[]? values, bool negate)
    {
        if (path.Length == 0)
        {
            // isNull tests the navigation a nested where was reached through, so there is nothing
            // to test at the root of a where.
            throw new($"isNull is only valid on a navigation. It was used at the root of the where for {typeof(T).Name}.");
        }

        try
        {
            Expression expressionBody;

            // If path includes list property access
            if (HasListPropertyInPath(path))
            {
                // Handle a list path
                expressionBody = ProcessList(path, comparison, values!);
            }
            // Otherwise linear property access
            else
            {
                // Just get expression
                expressionBody = GetExpression(path, comparison, values);
            }

            // If the expression should be negated
            if (negate)
            {
                expressionBody = NegateExpression(expressionBody);
            }

            return expressionBody;
        }
        catch (Exception exception)
        {
            throw new ($"Failed to build expression. Path: {path}, Comparison: {comparison}, Negate: {negate}, ", exception);
        }
    }

    /// <summary>
    /// Create a single predicate for the single set of supplied conditional arguments
    /// </summary>
    public static Expression<Func<T, bool>> BuildIdPredicate(string path, string[] values)
    {
        var expressionBody = MakeIdPredicateBody(path, values);
        var param = PropertyCache<T>.SourceParameter;

        return Expression.Lambda<Func<T, bool>>(expressionBody, param);
    }

    static Expression MakeIdPredicateBody(string path, string[] values)
    {
        try
        {
            return GetExpression(path, Comparison.In, values);
        }
        catch (Exception exception)
        {
            throw new ($"Failed to build expression. Path: {path} ", exception);
        }
    }

    static Expression ProcessList(string path, Comparison comparison, object?[]? values)
    {
        // Get the path pertaining to individual list items
        var listPath = ListPropertyRegex().Match(path).Groups[1].Value;
        // Remove the part of the path that leads into list item properties
        path = ListPropertyRegex().Replace(path, "");

        // Get the property on the current object up to the list member
        var property = PropertyCache<T>.GetProperty(path);

        // Get the list item type details
        var listItemType = property.PropertyType.GetGenericArguments().Single();

        var (buildPredicate, anyMethod) = ListMethods(listItemType);

        // Generate the predicate for the list item type. The string overload is the one reflected,
        // since a list path only comes from it.
        var subPredicate = (LambdaExpression)buildPredicate
            .Invoke(
                null,
                [
                    listPath,
                    comparison,
                    ToStrings(values),
                    false
                ])!;

        // The sub predicate is built on the parameter PropertyCache shares for the item type. When
        // the list holds the enclosing type, that is the same instance as the outer parameter, and
        // EF's parameter replacement then rewrites the inner lambda as well. Rebind the inner lambda
        // to its own parameter.
        var itemParameter = Expression.Parameter(listItemType, "item");
        var body = new ParameterReplacer(subPredicate.Parameters[0], itemParameter).Visit(subPredicate.Body);
        var itemPredicate = Expression.Lambda(body, itemParameter);

        // Create Any Expression Call
        return Expression.Call(anyMethod, property.Left, itemPredicate);
    }

    static string?[]? ToStrings(object?[]? values)
    {
        if (values is null)
        {
            return null;
        }

        if (values is string?[] strings)
        {
            return strings;
        }

        return values
            .Select(_ => _ is null ? null : Convert.ToString(_, CultureInfo.InvariantCulture))
            .ToArray();
    }

    /// <summary>
    /// The predicate on a collection navigation: Any, All or not Any of the items match.
    /// </summary>
    static Expression ProcessCollection(string path, Quantifier quantifier, IReadOnlyCollection<WhereExpression> wheres)
    {
        var property = PropertyCache<T>.GetProperty(path);
        if (!property.PropertyType.TryGetCollectionType(out var itemType))
        {
            throw new($"{path} is not a collection.");
        }

        var (any, all) = QuantifierMethods(itemType);

        LambdaExpression itemPredicate;
        var itemParameter = Expression.Parameter(itemType, "item");
        if (wheres.Count == 0)
        {
            // any: {} is has any items
            itemPredicate = Expression.Lambda(Expression.Constant(true), itemParameter);
        }
        else
        {
            // The sub predicate is built on the parameter PropertyCache shares for the item type,
            // so it is rebound to its own parameter, as ProcessList does
            var subPredicate = ExpressionBuilder.BuildPredicate(itemType, wheres);
            var body = new ParameterReplacer(subPredicate.Parameters[0], itemParameter).Visit(subPredicate.Body);
            itemPredicate = Expression.Lambda(body, itemParameter);
        }

        return quantifier switch
        {
            Quantifier.Any => Expression.Call(any, property.Left, itemPredicate),
            Quantifier.All => Expression.Call(all, property.Left, itemPredicate),
            Quantifier.None => Expression.Not(Expression.Call(any, property.Left, itemPredicate)),
            _ => throw new($"Unknown quantifier {quantifier}")
        };
    }

    static ConcurrentDictionary<Type, (MethodInfo Any, MethodInfo All)> quantifierMethods = new();

    static (MethodInfo Any, MethodInfo All) QuantifierMethods(Type itemType) =>
        quantifierMethods.GetOrAdd(
            itemType,
            type =>
            {
                var methods = typeof(Enumerable)
                    .GetMethods(BindingFlags.Static | BindingFlags.Public);
                var any = methods
                    .First(_ => _.Name == nameof(Enumerable.Any) &&
                                _.GetParameters().Length == 2)
                    .MakeGenericMethod(type);
                var all = methods
                    .First(_ => _.Name == nameof(Enumerable.All) &&
                                _.GetParameters().Length == 2)
                    .MakeGenericMethod(type);
                return (any, all);
            });

    static ConcurrentDictionary<Type, (MethodInfo BuildPredicate, MethodInfo Any)> listMethods = new();

    /// <summary>
    /// Both lookups were being repeated per where clause with a list path. The item types are
    /// bounded by the model, so they are safe to hold on to.
    /// </summary>
    static (MethodInfo BuildPredicate, MethodInfo Any) ListMethods(Type listItemType) =>
        listMethods.GetOrAdd(
            listItemType,
            type =>
            {
                var genericType = typeof(ExpressionBuilder<>).MakeGenericType(type);
                var buildPredicate = genericType
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .SingleOrDefault(_ => _.Name == nameof(BuildPredicate) &&
                                          _.GetParameters().Length == 4);
                if (buildPredicate == null)
                {
                    throw new($"Could not find BuildPredicate method on {genericType.FullName}");
                }

                var any = typeof(Enumerable)
                    .GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .First(_ => _.Name == nameof(Enumerable.Any) &&
                                _.GetParameters().Length == 2)
                    .MakeGenericMethod(type);

                return (buildPredicate, any);
            });

    static Expression GetExpression(string path, Comparison comparison, object?[]? values)
    {
        var property = PropertyCache<T>.GetProperty(path);
        Expression expression;

        if (property.PropertyType == typeof(string))
        {
            switch (comparison)
            {
                case Comparison.NotIn:
                    WhereValidator.ValidateString(comparison);
                    // Ensure expression is negated
                    expression = NegateExpression(MakeStringListInComparison(values!, property));
                    break;
                case Comparison.In:
                    WhereValidator.ValidateString(comparison);
                    expression = MakeStringListInComparison(values!, property);
                    break;

                default:
                    WhereValidator.ValidateSingleString(comparison);
                    var value = (string?) values?.Single();
                    expression = MakeSingleStringComparison(comparison, value, property);
                    break;
            }
        }
        else
        {
            switch (comparison)
            {
                case Comparison.NotIn:
                    WhereValidator.ValidateObject(property.PropertyType, comparison);
                    expression = NegateExpression(MakeObjectListInComparision(values!, property));
                    break;
                case Comparison.In:
                    WhereValidator.ValidateObject(property.PropertyType, comparison);
                    expression = MakeObjectListInComparision(values!, property);
                    break;

                default:
                    WhereValidator.ValidateSingleObject(property.PropertyType, comparison);
                    var value = values?.Single();
                    var valueObject = TypeConverter.ConvertValue(value, property.PropertyType);
                    expression = MakeSingleObjectComparison(comparison, valueObject, property);
                    break;
            }
        }

        return expression;
    }

    static MethodCallExpression MakeObjectListInComparision(object?[] values, Property<T> property)
    {
        var objects = TypeConverter.ConvertToList(values, property.Info);
        var constant = MakeParameterizedConstant(objects, objects.GetType());
        return Expression.Call(constant, property.SafeListContains, property.Left);
    }

    static MethodCallExpression MakeStringListInComparison(object?[] values, Property<T> property)
    {
        var strings = ToStrings(values)!;
        var equalsBody = Expression.Call(null, ReflectionCache.StringEqual, ExpressionCache.StringParam, property.Left);

        // Make lambda for comparing each string value against property value
        var itemEvaluate = Expression.Lambda<Func<string, bool>>(equalsBody, ExpressionCache.StringParam);

        // Build Expression body to check if any string values match the property value
        return Expression.Call(null, ReflectionCache.StringAny, MakeParameterizedConstant(strings, typeof(string[])), itemEvaluate);
    }

    static Expression MakeSingleStringComparison(Comparison comparison, string? value, Property<T> property)
    {
        var left = property.Left;

        var valueConstant = MakeParameterizedConstant(value, typeof(string));
        var nullCheck = Expression.NotEqual(left, ExpressionCache.Null);

        switch (comparison)
        {
            case Comparison.Equal:
                return Expression.Call(ReflectionCache.StringEqual, left, valueConstant);
            case Comparison.NotEqual:
                return Expression.Not(Expression.Call(ReflectionCache.StringEqual, left, valueConstant));
            case Comparison.Like:
                return Expression.Call(null, ReflectionCache.StringLike, ExpressionCache.EfFunction, left, valueConstant);
            case Comparison.StartsWith:
                var startsWithExpression = Expression.Call(left, ReflectionCache.StringStartsWith, valueConstant);
                return Expression.AndAlso(nullCheck, startsWithExpression);
            case Comparison.EndsWith:
                var endsWithExpression = Expression.Call(left, ReflectionCache.StringEndsWith, valueConstant);
                return Expression.AndAlso(nullCheck, endsWithExpression);
            case Comparison.Contains:
                var indexOfExpression = Expression.Call(left, ReflectionCache.StringIndexOf, valueConstant);
                var notEqualExpression = Expression.NotEqual(indexOfExpression, ExpressionCache.NegativeOne);
                return Expression.AndAlso(nullCheck, notEqualExpression);
        }

        throw new($"Invalid comparison operator '{comparison}'.");
    }

    static Expression MakeSingleObjectComparison(Comparison comparison, object? value, Property<T> property)
    {
        var left = property.Left;
        var constant = MakeParameterizedConstant(value, left.Type);

        return comparison switch
        {
            Comparison.Equal => Expression.MakeBinary(ExpressionType.Equal, left, constant),
            Comparison.NotEqual => Expression.MakeBinary(ExpressionType.NotEqual, left, constant),
            Comparison.GreaterThan => Expression.MakeBinary(ExpressionType.GreaterThan, left, constant),
            Comparison.GreaterThanOrEqual => Expression.MakeBinary(ExpressionType.GreaterThanOrEqual, left, constant),
            Comparison.LessThan => Expression.MakeBinary(ExpressionType.LessThan, left, constant),
            Comparison.LessThanOrEqual => Expression.MakeBinary(ExpressionType.LessThanOrEqual, left, constant),
            _ => throw new($"Invalid comparison operator '{comparison}'.")
        };
    }

    // Wraps a value in a field access on a holder object so that EF Core
    // emits a SQL parameter (@p0) instead of inlining the value as a literal.
    static Expression MakeParameterizedConstant(object? value, Type targetType)
    {
        var holder = new ValueHolder(value);
        Expression access = Expression.Field(Expression.Constant(holder), "Value");
        return Expression.Convert(access, targetType);
    }

    static bool HasListPropertyInPath(string path) =>
        path.Contains('[');

    static Expression CombineExpressions(Connector connector, Expression expr1, Expression expr2) =>
        connector switch
        {
            Connector.And => Expression.AndAlso(expr1, expr2),
            Connector.Or => Expression.OrElse(expr1, expr2),
            _ => throw new($"Invalid connector operator '{connector}'.")
        };

    static Expression NegateExpression(Expression expression) =>
        Expression.Not(expression);

    [GeneratedRegex(@"\[(.*)\]")]
    private static partial Regex ListPropertyRegex();
}

class ValueHolder(object? value)
{
    public object? Value = value;
}

/// <summary>
/// The predicate builders for an item type known only at runtime, for the collection subquery of
/// a navigation. The generic methods are looked up once per type.
/// </summary>
static class ExpressionBuilder
{
    static ConcurrentDictionary<Type, (MethodInfo Predicate, MethodInfo IdPredicate)> methods = new();

    static (MethodInfo Predicate, MethodInfo IdPredicate) Methods(Type type) =>
        methods.GetOrAdd(
            type,
            _ =>
            {
                var builder = typeof(ExpressionBuilder<>).MakeGenericType(_);
                return (
                    builder.GetMethod(nameof(ExpressionBuilder<>.BuildPredicate), [typeof(IReadOnlyCollection<WhereExpression>)])!,
                    builder.GetMethod(nameof(ExpressionBuilder<>.BuildIdPredicate), [typeof(string), typeof(string[])])!);
            });

    public static LambdaExpression BuildPredicate(Type type, IReadOnlyCollection<WhereExpression> wheres) =>
        Invoke(Methods(type).Predicate, [wheres]);

    public static LambdaExpression BuildIdPredicate(Type type, string keyName, string[] ids) =>
        Invoke(Methods(type).IdPredicate, [keyName, ids]);

    static LambdaExpression Invoke(MethodInfo method, object[] arguments)
    {
        try
        {
            return (LambdaExpression) method.Invoke(null, arguments)!;
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }
}
