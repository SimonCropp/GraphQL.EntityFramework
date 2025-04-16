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
        var previousWhere = new WhereExpression();

        // Iterate over wheres
        foreach (var where in wheres)
        {
            Expression nextExpression;

            // If there are grouped expressions
            if (where.GroupedExpressions?.Length > 0)
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
                mainExpression = CombineExpressions(previousWhere.Connector, mainExpression, nextExpression);
            }

            // Save the previous where so the connector can be retrieved
            previousWhere = where;
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

    static Expression MakePredicateBody(string path, Comparison comparison, string?[]? values, bool negate)
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

    static Expression ProcessList(string path, Comparison comparison, string?[]? values)
    {
        // Get the path pertaining to individual list items
        var listPath = ListPropertyRegex().Match(path).Groups[1].Value;
        // Remove the part of the path that leads into list item properties
        path = ListPropertyRegex().Replace(path, "");

        // Get the property on the current object up to the list member
        var property = PropertyCache<T>.GetProperty(path);

        // Get the list item type details
        var listItemType = property.PropertyType.GetGenericArguments().Single();

        // Generate the predicate for the list item type
        var genericType = typeof(ExpressionBuilder<>)
            .MakeGenericType(listItemType);
        var buildPredicate = genericType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(_ => _.Name == "BuildPredicate" &&
                                  _.GetParameters().Length == 4);
        if (buildPredicate == null)
        {
            throw new($"Could not find BuildPredicate method on {genericType.FullName}");
        }

        var subPredicate = (Expression)buildPredicate
            .Invoke(
                new(),
                [
                    listPath,
                    comparison,
                    values!,
                    false
                ])!;

        // Generate a method info for the Any Enumerable Static Method
        var anyInfo = typeof(Enumerable)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(_ => _.Name == "Any" &&
                        _.GetParameters().Length == 2)
            .MakeGenericMethod(listItemType);

        // Create Any Expression Call
        return Expression.Call(anyInfo, property.Left, subPredicate);
    }

    static Expression GetExpression(string path, Comparison comparison, string?[]? values)
    {
        var property = PropertyCache<T>.GetProperty(path);
        Expression expressionBody;

        if (property.PropertyType == typeof(string))
        {
            switch (comparison)
            {
                case Comparison.NotIn:
                    WhereValidator.ValidateString(comparison);
                    expressionBody = NegateExpression(MakeStringListInComparison(values!, property)); // Ensure expression is negated
                    break;
                case Comparison.In:
                    WhereValidator.ValidateString(comparison);
                    expressionBody = MakeStringListInComparison(values!, property);
                    break;

                default:
                    WhereValidator.ValidateSingleString(comparison);
                    var value = values?.Single();
                    expressionBody = MakeSingleStringComparison(comparison, value, property);
                    break;
            }
        }
        else
        {
            switch (comparison)
            {
                case Comparison.NotIn:
                    WhereValidator.ValidateObject(property.PropertyType, comparison);
                    expressionBody = NegateExpression(MakeObjectListInComparision(values!, property));
                    break;
                case Comparison.In:
                    WhereValidator.ValidateObject(property.PropertyType, comparison);
                    expressionBody = MakeObjectListInComparision(values!, property);
                    break;

                default:
                    WhereValidator.ValidateSingleObject(property.PropertyType, comparison);
                    var value = values?.Single();
                    var valueObject = TypeConverter.ConvertStringToType(value, property.PropertyType);
                    expressionBody = MakeSingleObjectComparison(comparison, valueObject, property);
                    break;
            }
        }

        return expressionBody;
    }

    static Expression MakeObjectListInComparision(string[] values, Property<T> property)
    {
        // Attempt to convert the string values to the object type
        var objects = TypeConverter.ConvertStringsToList(values, property.Info);
        // Make the object values a constant expression
        var constant = Expression.Constant(objects);
        // Build and return the expression body
        if (property.ListContains is null)
        {
            throw new($"No ListContains found for {typeof(T).Name}");
        }

        return Expression.Call(constant, property.ListContains, property.Left);
    }

    static Expression MakeStringListInComparison(string[] values, Property<T> property)
    {
        var equalsBody = Expression.Call(null, ReflectionCache.StringEqual, ExpressionCache.StringParam, property.Left);

        // Make lambda for comparing each string value against property value
        var itemEvaluate = Expression.Lambda<Func<string, bool>>(equalsBody, ExpressionCache.StringParam);

        // Build Expression body to check if any string values match the property value
        return Expression.Call(null, ReflectionCache.StringAny, Expression.Constant(values), itemEvaluate);
    }

    static Expression MakeSingleStringComparison(Comparison comparison, string? value, Property<T> property)
    {
        var left = property.Left;

        var valueConstant = Expression.Constant(value, typeof(string));
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
        var constant = Expression.Constant(value, left.Type);

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