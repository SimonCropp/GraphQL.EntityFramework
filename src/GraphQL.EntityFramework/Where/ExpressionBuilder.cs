using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using GraphQL.EntityFramework;
using Microsoft.EntityFrameworkCore;

static class ExpressionBuilder<T>
{
    static ConcurrentDictionary<string, PropertyAccessor> funcs = new ConcurrentDictionary<string, PropertyAccessor>();

    public static Expression<Func<T, bool>> BuildPredicate(WhereExpression where)
    {
        return BuildPredicate(where.Path, where.Comparison.GetValueOrDefault(), where.Value, where.Case);
    }

    public static Expression<Func<T, object>> BuildPropertyExpression(string path)
    {
        var propertyFunc = GetPropertyFunc(path);
        var propAsObject = Expression.Convert(propertyFunc.Left, typeof(object));

        return Expression.Lambda<Func<T, object>>(propAsObject, propertyFunc.SourceParameter);
    }

    public static Expression<Func<T, bool>> BuildPredicate(string path, Comparison comparison, string[] values, StringComparison? stringComparison = null)
    {
        var propertyFunc = GetPropertyFunc(path);

        if (propertyFunc.Type == typeof(string))
        {
            WhereValidator.ValidateString(comparison, stringComparison);
            switch (comparison)
            {
                case Comparison.In:
                    return BuildStringIn(values, propertyFunc, stringComparison);

                case Comparison.NotIn:
                    return BuildStringIn(values, propertyFunc, stringComparison, true);

                default:
                    var value = values?.Single();
                    return BuildStringCompare(comparison, value, propertyFunc, stringComparison);
            }
        }
        else
        {
            WhereValidator.ValidateObject(propertyFunc.Type, comparison, stringComparison);
            switch (comparison)
            {
                case Comparison.In:
                    return BuildObjectIn(values, propertyFunc);

                case Comparison.NotIn:
                    return BuildObjectIn(values, propertyFunc, true);

                default:
                    var value = values?.Single();
                    return BuildObjectCompare(comparison, value, propertyFunc);
            }
        }
    }

    public static Expression<Func<T, bool>> BuildSinglePredicate(string path, Comparison comparison, string value, StringComparison? stringComparison = null)
    {
        var propertyFunc = GetPropertyFunc(path);

        if (propertyFunc.Type == typeof(string))
        {
            WhereValidator.ValidateSingleString(comparison, stringComparison);
            return BuildStringCompare(comparison, value, propertyFunc, stringComparison);
        }

        WhereValidator.ValidateSingleObject(propertyFunc.Type, comparison, stringComparison);
        return BuildObjectCompare(comparison, value, propertyFunc);
    }

    static Expression<Func<T, bool>> BuildStringCompare(Comparison comparison, string value, PropertyAccessor propertyAccessor, StringComparison? stringComparison)
    {
        var body = MakeStringComparison(propertyAccessor.Left, comparison, value, stringComparison);
        return Expression.Lambda<Func<T, bool>>(body, propertyAccessor.SourceParameter);
    }

    static Expression<Func<T, bool>> BuildObjectCompare(Comparison comparison, string expressionValue, PropertyAccessor propertyAccessor)
    {
        var valueObject = TypeConverter.ConvertStringToType(expressionValue, propertyAccessor.Type);
        var body = MakeObjectComparison(propertyAccessor.Left, comparison, valueObject);
        return Expression.Lambda<Func<T, bool>>(body, propertyAccessor.SourceParameter);
    }

    static PropertyAccessor GetPropertyFunc(string propertyPath)
    {
        return funcs.GetOrAdd(propertyPath, x =>
        {
            var parameter = Expression.Parameter(typeof(T));
            var aggregatePath = AggregatePath(x, parameter);
            return new PropertyAccessor
            {
                SourceParameter = parameter,
                Left = aggregatePath,
                Type = aggregatePath.Type
            };
        });
    }

    static Expression<Func<T, bool>> BuildObjectIn(string[] values, PropertyAccessor propertyAccessor, bool not = false)
    {
        var objects = TypeConverter.ConvertStringsToList(values, propertyAccessor.Type);
        var constant = Expression.Constant(objects);
        var inInfo = objects.GetType().GetMethod("Contains", new[] {propertyAccessor.Type});
        var body = Expression.Call(constant, inInfo, propertyAccessor.Left);
        return Expression.Lambda<Func<T, bool>>(not ? Expression.Not(body) : (Expression)body, propertyAccessor.SourceParameter);
    }

    static Expression<Func<T, bool>> BuildStringIn(string[] array, PropertyAccessor propertyAccessor, StringComparison? stringComparison, bool not = false)
    {
        var itemValue = Expression.Parameter(typeof(string));
        MethodCallExpression equalsBody;
        if (stringComparison == null)
        {
            equalsBody = Expression.Call(null, StringMethodCache.Equal, itemValue, propertyAccessor.Left);
        }
        else
        {
            equalsBody = Expression.Call(null, StringMethodCache.EqualComparison, itemValue, propertyAccessor.Left, Expression.Constant(stringComparison));
        }
        var itemEvaluate = Expression.Lambda<Func<string, bool>>(equalsBody, itemValue);
        var anyBody = Expression.Call(null, StringMethodCache.Any, Expression.Constant(array), itemEvaluate);
        return Expression.Lambda<Func<T, bool>>(not ? Expression.Not(anyBody) : (Expression)anyBody, propertyAccessor.SourceParameter);
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
                    return Expression.Call(StringMethodCache.Equal, left, valueConstant);
                case Comparison.Like:
                    return Expression.Call(null, StringMethodCache.Like, Expression.Constant(EF.Functions), left, valueConstant);
                case Comparison.NotEqual:
                    var notEqualsCall = Expression.Call(StringMethodCache.Equal, left, valueConstant);
                    return Expression.Not(notEqualsCall);
                case Comparison.StartsWith:
                    var startsWithExpression = Expression.Call(left, StringMethodCache.StartsWith, valueConstant);
                    return Expression.AndAlso(nullCheck, startsWithExpression);
                case Comparison.EndsWith:
                    var endsWithExpression = Expression.Call(left, StringMethodCache.EndsWith, valueConstant);
                    return Expression.AndAlso(nullCheck, endsWithExpression);
                case Comparison.Contains:
                    var indexOfExpression = Expression.Call(left, StringMethodCache.IndexOf, valueConstant);
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
                    return Expression.Call(StringMethodCache.EqualComparison, left, valueConstant, comparisonConstant);
                case Comparison.NotEqual:
                    var notEqualsCall = Expression.Call(StringMethodCache.EqualComparison, left, valueConstant, comparisonConstant);
                    return Expression.Not(notEqualsCall);
                case Comparison.StartsWith:
                    var startsWithExpression = Expression.Call(left, StringMethodCache.StartsWithComparison, valueConstant, comparisonConstant);
                    return Expression.AndAlso(nullCheck, startsWithExpression);
                case Comparison.EndsWith:
                    var endsWithExpression = Expression.Call(left, StringMethodCache.EndsWithComparison, valueConstant, comparisonConstant);
                    return Expression.AndAlso(nullCheck, endsWithExpression);
                case Comparison.Contains:
                    var indexOfExpression = Expression.Call(left, StringMethodCache.IndexOfComparison, valueConstant, comparisonConstant);
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

    internal static Expression AggregatePath(string path, Expression parameter)
    {
        try
        {
            return path.Split('.')
                .Aggregate(parameter, Expression.PropertyOrField);
        }
        catch (ArgumentException exception)
        {
            throw new Exception($"Failed to create a member expression. Type: {typeof(T).FullName}, Path: {path}. Error: {exception.Message}");
        }
    }

    class PropertyAccessor
    {
        public ParameterExpression SourceParameter;
        public Expression Left;
        public Type Type;
    }
}