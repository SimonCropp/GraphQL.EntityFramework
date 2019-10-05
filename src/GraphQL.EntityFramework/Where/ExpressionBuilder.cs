using System;
using System.Linq;
using System.Linq.Expressions;

namespace GraphQL.EntityFramework
{
    public static class ExpressionBuilder<T>
    {
        public static Expression<Func<T, bool>> BuildPredicate(WhereExpression where)
        {
            Guard.AgainstNull(nameof(where), where);
            return BuildPredicate(where.Path, where.Comparison.GetValueOrDefault(), where.Value, where.Case);
        }

        internal static Expression<Func<T, bool>> BuildPredicate(string path, Comparison comparison, string?[]? values, StringComparison? stringComparison = null)
        {
            var property = PropertyCache<T>.GetProperty(path);

            if (property.PropertyType == typeof(string))
            {
                WhereValidator.ValidateString(comparison, stringComparison);
                switch (comparison)
                {
                    case Comparison.In:
                        return BuildStringIn(values!, property, stringComparison);

                    case Comparison.NotIn:
                        return BuildStringIn(values!, property, stringComparison, true);

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
                        return BuildObjectIn(values!, property);

                    case Comparison.NotIn:
                        return BuildObjectIn(values!, property, true);

                    default:
                        var value = values?.Single();
                        return BuildObjectCompare(comparison, value, property);
                }
            }
        }

        internal static Expression<Func<T, bool>> BuildSinglePredicate(string path, Comparison comparison, string value, StringComparison? stringComparison = null)
        {
            var property = PropertyCache<T>.GetProperty(path);

            if (property.PropertyType == typeof(string))
            {
                WhereValidator.ValidateSingleString(comparison, stringComparison);
                return BuildStringCompare(comparison, value, property, stringComparison);
            }

            WhereValidator.ValidateSingleObject(property.PropertyType, comparison, stringComparison);
            return BuildObjectCompare(comparison, value, property);
        }

        static Expression<Func<T, bool>> BuildStringCompare(Comparison comparison, string? value, Property<T> property, StringComparison? stringComparison)
        {
            var body = MakeStringComparison(property.Left, comparison, value, stringComparison);
            return Expression.Lambda<Func<T, bool>>(body, property.SourceParameter);
        }

        static Expression<Func<T, bool>> BuildObjectCompare(Comparison comparison, string? value, Property<T> property)
        {
            var valueObject = TypeConverter.ConvertStringToType(value, property.PropertyType);
            var body = MakeObjectComparison(property.Left, comparison, valueObject);
            return Expression.Lambda<Func<T, bool>>(body, property.SourceParameter);
        }

        static Expression<Func<T, bool>> BuildObjectIn(string[] values, Property<T> property, bool not = false)
        {
            var objects = TypeConverter.ConvertStringsToList(values, property.PropertyType);
            var constant = Expression.Constant(objects);
            var body = Expression.Call(constant, property.ListContainsMethod, property.Left);
            return Expression.Lambda<Func<T, bool>>(not ? Expression.Not(body) : (Expression) body, property.SourceParameter);
        }

        static Expression<Func<T, bool>> BuildStringIn(string[] array, Property<T> property, StringComparison? comparison, bool not = false)
        {
            MethodCallExpression equalsBody;
            if (comparison == null)
            {
                equalsBody = Expression.Call(null, ReflectionCache.StringEqual, ExpressionCache.StringParam, property.Left);
            }
            else
            {
                equalsBody = Expression.Call(null, ReflectionCache.StringEqualComparison, ExpressionCache.StringParam, property.Left, Expression.Constant(comparison));
            }

            var itemEvaluate = Expression.Lambda<Func<string, bool>>(equalsBody, ExpressionCache.StringParam);
            var anyBody = Expression.Call(null, ReflectionCache.StringAny, Expression.Constant(array), itemEvaluate);
            return Expression.Lambda<Func<T, bool>>(not ? Expression.Not(anyBody) : (Expression) anyBody, property.SourceParameter);
        }

        static Expression MakeStringComparison(Expression left, Comparison comparison, string? value, StringComparison? stringComparison)
        {
            var valueConstant = Expression.Constant(value, typeof(string));
            var nullCheck = Expression.NotEqual(left, ExpressionCache.Null);

            if (stringComparison == null)
            {
                switch (comparison)
                {
                    case Comparison.Equal:
                        return Expression.Call(ReflectionCache.StringEqual, left, valueConstant);
                    case Comparison.Like:
                        return Expression.Call(null, ReflectionCache.StringLike, ExpressionCache.EfFunction, left, valueConstant);
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
                        var notEqualExpression = Expression.NotEqual(indexOfExpression, ExpressionCache.NegativeOne);
                        return Expression.AndAlso(nullCheck, notEqualExpression);
                }

            }

            throw new NotSupportedException($"Invalid comparison operator '{comparison}'.");
        }

        static Expression MakeObjectComparison(Expression left, Comparison comparison, object? value)
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
}