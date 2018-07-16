using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GraphQL.EntityFramework;

static class ExpressionBuilder<T>
{
    static ConcurrentDictionary<string, PropertyAccessor> funcs = new ConcurrentDictionary<string, PropertyAccessor>();

    public static Expression<Func<T, bool>> BuildPredicate(WhereExpression where)
    {
        return BuildPredicate2(where.Path, where.Comparison, where.Value, where.Case);
    }

    public static Expression<Func<T, bool>> BuildPredicate2(string path, Comparison comparison, string[] values, StringComparison? stringComparison = null)
    {
        var propertyFunc = GetPropertyFunc(path);
        var parameter = Expression.Parameter(typeof(T));
        var left = AggregatePath(path, parameter);

        if (left.Type == typeof(string))
        {
            WhereValidator.ValidateString(comparison, stringComparison);
            //var stringComparisonValue = stringComparison.GetValueOrDefault(StringComparison.OrdinalIgnoreCase);
            if (comparison == Comparison.In)
            {
                return BuildStringIn(values, propertyFunc);
            }
            else
            {
                var value = values?.Single();
                return BuildStringCompare(comparison, value, propertyFunc);
            }
        }
        else
        {
            WhereValidator.ValidateObject(left.Type, comparison, stringComparison);
            if (comparison == Comparison.In)
            {
                return BuildObjectIn(values, propertyFunc);
            }
            else
            {
                var value = values?.Single();
                return BuildObjectCompare(comparison, value, propertyFunc);
            }
        }
    }

    static Expression<Func<T, bool>> BuildStringCompare(Comparison comparison, string value, PropertyAccessor propertyAccessor)
    {
        var body = MakeStringComparison(propertyAccessor.Left, comparison, value);
        return Expression.Lambda<Func<T, bool>>(body, propertyAccessor.Parameter);
    }

    static Expression<Func<T, bool>> BuildObjectCompare(Comparison comparison, string expressionValue, PropertyAccessor propertyAccessor)
    {
        var valueObject = TypeConverter.ConvertStringToType(expressionValue, propertyAccessor.Left.Type);
        var body = MakeObjectComparison(propertyAccessor.Left, comparison, valueObject);
        return Expression.Lambda<Func<T, bool>>(body, propertyAccessor.Parameter);
    }

    static PropertyAccessor GetPropertyFunc(string propertyPath)
    {
        return funcs.GetOrAdd(propertyPath, x =>
        {
            var parameter = Expression.Parameter(typeof(T));
            return new PropertyAccessor
            {
                Parameter = parameter,
                Left = AggregatePath(x, parameter)
            };
        });
    }

    static Expression<Func<T, bool>> BuildObjectIn(string[] values, PropertyAccessor propertyAccessor)
    {
        var objects = values.Select(x => TypeConverter.ConvertStringToType(x, propertyAccessor.Left.Type)).ToList();
        var constant = Expression.Constant(objects);
        var inInfo = objects.GetType().GetMethod("Contains", new[] {propertyAccessor.Left.Type});
        var body = Expression.Call(constant, inInfo, propertyAccessor.Left);
        return Expression.Lambda<Func<T, bool>>(body, propertyAccessor.Parameter);
    }

    static Expression<Func<T, bool>> BuildStringIn(string[] values, PropertyAccessor propertyAccessor)
    {
        var constant = Expression.Constant(values.ToList());
        var inInfo = typeof(List<string>).GetMethod("Contains", new[] {typeof(string)});
        var body = Expression.Call(constant, inInfo, propertyAccessor.Left);
        return Expression.Lambda<Func<T, bool>>(body, propertyAccessor.Parameter);
    }

    static Expression MakeStringComparison(Expression left, Comparison comparison, string value)
    {
        var constant = Expression.Constant(value, left.Type);
        switch (comparison)
        {
            case Comparison.Equal:
                return Expression.MakeBinary(ExpressionType.Equal, left, constant);
            case Comparison.NotEqual:
                return Expression.MakeBinary(ExpressionType.NotEqual, left, constant);
            case Comparison.GreaterThan:
            case Comparison.Contains:
            case Comparison.StartsWith:
            case Comparison.EndsWith:
                return Expression.Call(left, comparison.ToString(), Type.EmptyTypes, constant);
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

    static Expression AggregatePath(string propertyPath, Expression parameter)
    {
        return propertyPath.Split('.')
            .Aggregate(parameter, Expression.PropertyOrField);
    }

    class PropertyAccessor
    {
        public ParameterExpression Parameter;
        public Expression Left;
    }
}