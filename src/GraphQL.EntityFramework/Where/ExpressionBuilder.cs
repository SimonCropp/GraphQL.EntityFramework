namespace GraphQL.EntityFramework;

public static class ExpressionBuilder<T>
{
    const string listPropertyPattern = @"\[(.*)\]";

    /// <summary>
    /// Build a predicate for a supplied list of where's (Grouped or not)
    /// </summary>
    public static Expression<Func<T, bool>> BuildPredicate(IEnumerable<WhereExpression> wheres)
    {
        var expressionBody = MakePredicateBody(wheres);
        var param = PropertyCache<T>.SourceParameter;

        return Expression.Lambda<Func<T, bool>>(expressionBody, param);
    }

    static Expression MakePredicateBody(IEnumerable<WhereExpression> wheres)
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
                nextExpression = MakePredicateBody(where.Path, where.Comparison, where.Value, where.Negate, where.Case);
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
    public static Expression<Func<T, bool>> BuildPredicate(string path, Comparison comparison, string?[]? values, bool negate = false, StringComparison? stringComparison = null)
    {
        var expressionBody = MakePredicateBody(path, comparison, values, negate, stringComparison);
        var param = PropertyCache<T>.SourceParameter;

        return Expression.Lambda<Func<T, bool>>(expressionBody, param);
    }

    static Expression MakePredicateBody(string path, Comparison comparison, string?[]? values, bool negate, StringComparison? stringComparison)
    {
        Expression expressionBody;

        // If path includes list property access
        if (HasListPropertyInPath(path))
        {
            // Handle a list path
            expressionBody = ProcessList(path, comparison, values!, stringComparison);
        }
        // Otherwise linear property access
        else
        {
            // Just get expression
            expressionBody = GetExpression(path, comparison, values, stringComparison);
        }

        // If the expression should be negated
        if (negate)
        {
            expressionBody = NegateExpression(expressionBody);
        }

        return expressionBody;
    }

    static Expression ProcessList(string path, Comparison comparison, string?[]? values, StringComparison? stringComparison)
    {
        // Get the path pertaining to individual list items
        var listPath = Regex.Match(path, listPropertyPattern).Groups[1].Value;
        // Remove the part of the path that leads into list item properties
        path = Regex.Replace(path, listPropertyPattern, "");

        // Get the property on the current object up to the list member
        var property = PropertyCache<T>.GetProperty(path);

        // Get the list item type details
        var listItemType = property.PropertyType.GetGenericArguments().Single();

        // Generate the predicate for the list item type
        var subPredicate = (Expression) typeof(ExpressionBuilder<>)
            .MakeGenericType(listItemType)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == "BuildPredicate" && m.GetParameters().Length == 5)
            .Invoke(new(), new object[]
            {
                listPath,
                comparison,
                values!,
                false,
                stringComparison!
            })!;

        // Generate a method info for the Any Enumerable Static Method
        var anyInfo = typeof(Enumerable)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
            .MakeGenericMethod(listItemType);

        // Create Any Expression Call
        return Expression.Call(anyInfo, property.Left, subPredicate);
    }

    static Expression GetExpression(string path, Comparison comparison, string?[]? values, StringComparison? stringComparison)
    {
        var property = PropertyCache<T>.GetProperty(path);
        Expression expressionBody;

        if (property.PropertyType == typeof(string))
        {
            switch (comparison)
            {
                case Comparison.NotIn:
                    WhereValidator.ValidateString(comparison, stringComparison);
                    expressionBody = NegateExpression(MakeStringListInComparison(values!, property, stringComparison)); // Ensure expression is negated
                    break;
                case Comparison.In:
                    WhereValidator.ValidateString(comparison, stringComparison);
                    expressionBody = MakeStringListInComparison(values!, property, stringComparison);
                    break;

                default:
                    WhereValidator.ValidateSingleString(comparison, stringComparison);
                    var value = values?.Single();
                    expressionBody = MakeSingleStringComparison(comparison, value, property, stringComparison);
                    break;
            }
        }
        else
        {
            switch (comparison)
            {
                case Comparison.NotIn:
                    WhereValidator.ValidateObject(property.PropertyType, comparison, stringComparison);
                    expressionBody = NegateExpression(MakeObjectListInComparision(values!, property));
                    break;
                case Comparison.In:
                    WhereValidator.ValidateObject(property.PropertyType, comparison, stringComparison);
                    expressionBody = MakeObjectListInComparision(values!, property);
                    break;

                default:
                    WhereValidator.ValidateSingleObject(property.PropertyType, comparison, null);
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
        return Expression.Call(constant, property.ListContainsMethod!, property.Left);
    }

    static Expression MakeStringListInComparison(string[] values, Property<T> property, StringComparison? comparison)
    {
        MethodCallExpression equalsBody;

        if (comparison is null)
        {
            // Do basic string compare
            equalsBody = Expression.Call(null, ReflectionCache.StringEqual, ExpressionCache.StringParam, property.Left);
        }
        else
        {
            // String comparison with comparison type value
            equalsBody = Expression.Call(null, ReflectionCache.StringEqualComparison, ExpressionCache.StringParam, property.Left, Expression.Constant(comparison));
        }

        // Make lambda for comparing each string value against property value
        var itemEvaluate = Expression.Lambda<Func<string, bool>>(equalsBody, ExpressionCache.StringParam);

        // Build Expression body to check if any string values match the property value
        return Expression.Call(null, ReflectionCache.StringAny, Expression.Constant(values), itemEvaluate);
    }

    static Expression MakeSingleStringComparison(Comparison comparison, string? value, Property<T> property, StringComparison? stringComparison)
    {
        var left = property.Left;

        var valueConstant = Expression.Constant(value, typeof(string));
        var nullCheck = Expression.NotEqual(left, ExpressionCache.Null);

        if (stringComparison is null)
        {
            switch (comparison)
            {
                case Comparison.Equal:
                    return Expression.Call(ReflectionCache.StringEqual, left, valueConstant);
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
        }
        else
        {
            var comparisonConstant = Expression.Constant(stringComparison, typeof(StringComparison));
            switch (comparison)
            {
                case Comparison.Equal:
                    return Expression.Call(ReflectionCache.StringEqualComparison, left, valueConstant, comparisonConstant);
                case Comparison.StartsWith:
                    var startsWithExpression = Expression.Call(left, ReflectionCache.StringStartsWithComparison, valueConstant, comparisonConstant);
                    return Expression.AndAlso(nullCheck, startsWithExpression);
                case Comparison.EndsWith:
                    var endsWithExpression = Expression.Call(left, ReflectionCache.StringEndsWithComparison, valueConstant, comparisonConstant);
                    return Expression.AndAlso(nullCheck, endsWithExpression);
                case Comparison.Contains:
                    var indexOfExpression = Expression.Call(left, ReflectionCache.StringIndexOfComparison, valueConstant, comparisonConstant);
                    var notEqualExpression = Expression.NotEqual(indexOfExpression, ExpressionCache.NegativeOne);
                    return Expression.AndAlso(nullCheck, notEqualExpression);
            }
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
}