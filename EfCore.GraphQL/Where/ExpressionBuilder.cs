using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EfCoreGraphQL
{
    public static class ExpressionBuilder
    {
        public static Expression<Func<T, bool>> BuildPredicate<T>(string propertyPath, Comparison comparison, object value)
        {
            var parameter = Expression.Parameter(typeof(T));
            var left = AggregatePath(propertyPath, parameter);
            if (left.Type != typeof(string) && value is string stringValue)
            {
                value = ConvertStringToType(stringValue, left.Type);
            }

            var body = MakeComparison(left, comparison, value);
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        public static Expression<Func<T, bool>> BuildPredicate<T>(WhereExpression whereExpression)
        {
            if (whereExpression.Comparison == Comparison.In)
            {
                return BuildIn<T>(whereExpression.Path,whereExpression.Value);
            }
            return BuildCompare<T>(whereExpression);
        }

        static Expression<Func<T, bool>> BuildCompare<T>(WhereExpression whereExpression)
        {
            var parameter = Expression.Parameter(typeof(T));
            var left = AggregatePath(whereExpression.Path, parameter);

            var single = whereExpression.Value.Single();
            object value;
            if (left.Type == typeof(string))
            {
                value = single;
            }
            else
            {
                value = ConvertStringToType(single, left.Type);
            }

            var body = MakeComparison(left, whereExpression.Comparison, value);
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

      public static Expression<Func<T, bool>> BuildIn<T>(string propertyPath, IEnumerable<string> values)
        {
            var parameter = Expression.Parameter(typeof(T));
            var left = AggregatePath(propertyPath, parameter);
            var objects = values.Select(x=> ConvertStringToType(x,left.Type)).ToList();
            var constant = Expression.Constant(objects);
            var inInfo = objects.GetType().GetMethod("Contains", new[] { left.Type });
            var body = Expression.Call(constant, inInfo, left);
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        public static object ConvertStringToType(string value, Type type)
        {
            if (type == typeof(Guid))
            {
                return Guid.Parse(value);
            }

            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                if (value == null)
                {
                    return null;
                }

                type = underlyingType;
            }

            return Convert.ChangeType(value, type);
        }

        static Expression MakeComparison(Expression left, Comparison comparison, object value)
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
                case Comparison.Contains:
                case Comparison.StartsWith:
                case Comparison.EndsWith:
                    if (value is string)
                    {
                        return Expression.Call(left, comparison.ToString(), Type.EmptyTypes, constant);
                    }

                    throw new NotSupportedException($"Comparison operator '{comparison}' only supported on string.");
            }

            throw new NotSupportedException($"Invalid comparison operator '{comparison}'.");
        }

        static Expression AggregatePath(string propertyPath, Expression parameter)
        {
            return propertyPath.Split('.')
                .Aggregate(parameter, Expression.PropertyOrField);
        }


        //public static Expression<Func<T, bool>> BuildInForInstances<T>(string propertyPath, IList value)
        //{
        //    var parameter = Expression.Parameter(typeof(T));
        //    var left = AggregatePath(propertyPath, parameter);
        //    var listType = value.GetType();
        //    var constant = Expression.Constant(value, listType);
        //    var itemType = listType.GetGenericArguments()[0];
        //    if (itemType != left.Type)
        //    {
        //        throw new Exception($"PropertyPath ({propertyPath}) and list type do not match. PropertyPath Type: {left.Type.FullName}. List Type: {itemType.FullName}.");
        //    }

        //    var inInfo = listType.GetMethod("Contains", new[] { itemType });
        //    var body = Expression.Call(constant, inInfo, left);
        //    return Expression.Lambda<Func<T, bool>>(body, parameter);
        //}
    }
}