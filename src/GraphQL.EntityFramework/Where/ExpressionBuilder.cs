using System;
using System.Linq;
using System.Linq.Expressions;
using GraphQL.EntityFramework;
using Microsoft.EntityFrameworkCore;

static class ExpressionBuilder<T>
{
    public static Expression<Func<T, bool>> BuildPredicate(WhereExpression where)
    {
        return BuildPredicate(where.Path, where.Comparison.GetValueOrDefault(), where.Value, where.Case);
    }

    public static Expression<Func<T, object>> BuildPropertyExpression(string path)
    {
        var property = PropertyAccessorBuilder<T>.GetProperty(path);
        var propAsObject = Expression.Convert(property.Left, typeof(object));

        return Expression.Lambda<Func<T, object>>(propAsObject, property.SourceParameter);
    }

    public static Expression<Func<T, bool>> BuildPredicate(string path, Comparison comparison, string[] values, StringComparison? stringComparison = null)
    {
        var property = PropertyAccessorBuilder<T>.GetProperty(path);

        if (property.PropertyType == typeof(string))
        {
            WhereValidator.ValidateString(comparison, stringComparison);
            switch (comparison)
            {
                case Comparison.In:
                    return BuildStringIn(values, property, stringComparison);

                case Comparison.NotIn:
                    return BuildStringIn(values, property, stringComparison, true);

                default:
                    var value = values?.Single();
                    return BuildStringCompare(comparison, value, property, stringComparison);
            }
        }
        else
        {
            WhereValidator.ValidateObject(property.PropertyType, comparison, stringComparison);
            switch (comparison)
            {
                case Comparison.In:
                    return BuildObjectIn(values, property);

                case Comparison.NotIn:
                    return BuildObjectIn(values, property, true);

                default:
                    var value = values?.Single();
                    return BuildObjectCompare(comparison, value, property);
            }
        }
    }

    public static Expression<Func<T, bool>> BuildSinglePredicate(string path, Comparison comparison, string value, StringComparison? stringComparison = null)
    {
        var property = PropertyAccessorBuilder<T>.GetProperty(path);

        if (property.PropertyType == typeof(string))
        {
            WhereValidator.ValidateSingleString(comparison, stringComparison);
            return BuildStringCompare(comparison, value, property, stringComparison);
        }

        WhereValidator.ValidateSingleObject(property.PropertyType, comparison, stringComparison);
        return BuildObjectCompare(comparison, value, property);
    }

    static Expression<Func<T, bool>> BuildStringCompare(Comparison comparison, string value, Property<T> propertyExpression, StringComparison? stringComparison)
    {
        var body = MakeStringComparison(propertyExpression.Left, comparison, value, stringComparison);
        return Expression.Lambda<Func<T, bool>>(body, propertyExpression.SourceParameter);
    }

    static Expression<Func<T, bool>> BuildObjectCompare(Comparison comparison, string expressionValue, Property<T> propertyExpression)
    {
        var valueObject = TypeConverter.ConvertStringToType(expressionValue, propertyExpression.PropertyType);
        var body = MakeObjectComparison(propertyExpression.Left, comparison, valueObject);
        return Expression.Lambda<Func<T, bool>>(body, propertyExpression.SourceParameter);
    }

    static Expression<Func<T, bool>> BuildObjectIn(string[] values, Property<T> propertyExpression, bool not = false)
    {
        var objects = TypeConverter.ConvertStringsToList(values, propertyExpression.PropertyType);
        var constant = Expression.Constant(objects);
        var inInfo = ReflectionCache.GetListContains(propertyExpression.PropertyType);
        var body = Expression.Call(constant, inInfo, propertyExpression.Left);
        return Expression.Lambda<Func<T, bool>>(not ? Expression.Not(body) : (Expression)body, propertyExpression.SourceParameter);
    }

    static Expression<Func<T, bool>> BuildStringIn(string[] array, Property<T> propertyExpression, StringComparison? stringComparison, bool not = false)
    {
        var itemValue = Expression.Parameter(typeof(string));
        MethodCallExpression equalsBody;
        if (stringComparison == null)
        {
            equalsBody = Expression.Call(null, ReflectionCache.StringEqual, itemValue, propertyExpression.Left);
        }
        else
        {
            equalsBody = Expression.Call(null, ReflectionCache.StringEqualComparison, itemValue, propertyExpression.Left, Expression.Constant(stringComparison));
        }
        var itemEvaluate = Expression.Lambda<Func<string, bool>>(equalsBody, itemValue);
        var anyBody = Expression.Call(null, ReflectionCache.StringAny, Expression.Constant(array), itemEvaluate);
        return Expression.Lambda<Func<T, bool>>(not ? Expression.Not(anyBody) : (Expression)anyBody, propertyExpression.SourceParameter);
    }

    static Expression MakeStringComparison(Expression left, Comparison comparison, string value, StringComparison? stringComparison)
    {
        var valueConstant = Expression.Constant(value, typeof(string));
        var nullCheck = Expression.NotEqual(left, Expression.Constant(null, typeof(object)));
        if (stringComparison == null)
        {
            switch (comparison)
            {
                case Comparison.Equal:
                    return Expression.Call(ReflectionCache.StringEqual, left, valueConstant);
                case Comparison.Like:
                    return Expression.Call(null, ReflectionCache.StringLike, Expression.Constant(EF.Functions), left, valueConstant);
                case Comparison.NotEqual:
                    var notEqualsCall = Expression.Call(ReflectionCache.StringEqual, left, valueConstant);
                    return Expression.Not(notEqualsCall);
                case Comparison.StartsWith:
                    var startsWithExpression = Expression.Call(left, ReflectionCache.StringStartsWith, valueConstant);
                    return Expression.AndAlso(nullCheck, startsWithExpression);
                case Comparison.EndsWith:
                    var endsWithExpression = Expression.Call(left, ReflectionCache.StringEndsWith, valueConstant);
                    return Expression.AndAlso(nullCheck, endsWithExpression);
                case Comparison.Contains:
                    var indexOfExpression = Expression.Call(left, ReflectionCache.StringIndexOf, valueConstant);
                    var notEqualExpression = Expression.NotEqual(indexOfExpression, Expression.Constant(-1));
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
                case Comparison.NotEqual:
                    var notEqualsCall = Expression.Call(ReflectionCache.StringEqualComparison, left, valueConstant, comparisonConstant);
                    return Expression.Not(notEqualsCall);
                case Comparison.StartsWith:
                    var startsWithExpression = Expression.Call(left, ReflectionCache.StringStartsWithComparison, valueConstant, comparisonConstant);
                    return Expression.AndAlso(nullCheck, startsWithExpression);
                case Comparison.EndsWith:
                    var endsWithExpression = Expression.Call(left, ReflectionCache.StringEndsWithComparison, valueConstant, comparisonConstant);
                    return Expression.AndAlso(nullCheck, endsWithExpression);
                case Comparison.Contains:
                    var indexOfExpression = Expression.Call(left, ReflectionCache.StringIndexOfComparison, valueConstant, comparisonConstant);
                    var notEqualExpression = Expression.NotEqual(indexOfExpression, Expression.Constant(-1));
                    return Expression.AndAlso(nullCheck, notEqualExpression);
            }

        }

        throw new NotSupportedException($"Invalid comparison operator '{comparison}'.");
    }

    static Expression MakeObjectComparison(Expression left, Comparison comparison, object value)
    {
        var constant = Expression.Constant(value, left.Type);
        switch (comparison)
        {
            case Comparison.Equal:
                return Expression.MakeBinary(ExpressionType.Equal, left, constant);
            case Comparison.NotEqual:
                return Expression.MakeBinary(ExpressionType.NotEqual, left, constant);
            case Comparison.GreaterThan:
                return Expression.MakeBinary(ExpressionType.GreaterThan, left, constant);
            case Comparison.GreaterThanOrEqual:
                return Expression.MakeBinary(ExpressionType.GreaterThanOrEqual, left, constant);
            case Comparison.LessThan:
                return Expression.MakeBinary(ExpressionType.LessThan, left, constant);
            case Comparison.LessThanOrEqual:
                return Expression.MakeBinary(ExpressionType.LessThanOrEqual, left, constant);
        }

        throw new NotSupportedException($"Invalid comparison operator '{comparison}'.");
    }
}