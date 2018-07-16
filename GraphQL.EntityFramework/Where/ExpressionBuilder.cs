using System;
using System.Collections.Concurrent;
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
            var stringComparisonValue = stringComparison.GetValueOrDefault(StringComparison.OrdinalIgnoreCase);
            if (comparison == Comparison.In)
            {
                return BuildStringIn(values, propertyFunc, stringComparisonValue);
            }
            else
            {
                var value = values?.Single();
                return BuildStringCompare(comparison, value, propertyFunc, stringComparisonValue);
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

    static Expression<Func<T, bool>> BuildStringCompare(Comparison comparison, string value, PropertyAccessor propertyAccessor, StringComparison stringComparison)
    {
        var body = MakeStringComparison(propertyAccessor.Left, comparison, value, stringComparison);
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

    private sealed class c__DisplayClass0_0
    {
        public string[] array;

    public StringComparison stringComparison;
}

private static Expression<Func<Foo, bool>> InExpression(string[] array, StringComparison stringComparison)
{
         c__DisplayClass0_0 c__DisplayClass0_ = new <> c__DisplayClass0_0();
         c__DisplayClass0_.array = array;
         c__DisplayClass0_.stringComparison = stringComparison;
    ParameterExpression parameterExpression = Expression.Parameter(typeof(Foo), "x");
    MethodInfo method = (MethodInfo)MethodBase.GetMethodFromHandle((RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/);
    Expression[] obj = new Expression[2];
    obj[0] = Expression.Field(Expression.Constant(<> c__DisplayClass0_, typeof(c__DisplayClass0_0)), FieldInfo.GetFieldFromHandle((RuntimeFieldHandle)/*OpCode not supported: LdMemberToken*/));
    ParameterExpression parameterExpression2 = Expression.Parameter(typeof(string), "y");
    MethodInfo method2 = (MethodInfo)MethodBase.GetMethodFromHandle((RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/);
    Expression[] obj2 = new Expression[3];
    obj2[0] = Expression.Property(parameterExpression, (MethodInfo)MethodBase.GetMethodFromHandle((RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/));
    obj2[1] = parameterExpression2;
    obj2[2] = Expression.Field(Expression.Constant(<> c__DisplayClass0_, typeof( c__DisplayClass0_0)), FieldInfo.GetFieldFromHandle((RuntimeFieldHandle)/*OpCode not supported: LdMemberToken*/));
    MethodCallExpression body = Expression.Call(null, method2, obj2);
    ParameterExpression[] obj3 = new ParameterExpression[1];
    obj3[0] = parameterExpression2;
    obj[1] = Expression.Lambda<Func<string, bool>>((Expression)body, obj3);
    MethodCallExpression body2 = Expression.Call(null, method, obj);
    ParameterExpression[] obj4 = new ParameterExpression[1];
    obj4[0] = parameterExpression;
    return Expression.Lambda<Func<Foo, bool>>((Expression)body2, obj4);
}


static Expression<Func<T, bool>> BuildStringIn(string[] array, PropertyAccessor propertyAccessor, StringComparison stringComparison)
{
    ParameterExpression parameterExpression2 = Expression.Parameter(typeof(string), "y");
        var equalsBody = Expression.Call(null, StringMethodCache.Equal, propertyAccessor.Left, parameterExpression2, Expression.Constant(stringComparison));

    var expression = Expression.Lambda<Func<string, bool>>(equalsBody, propertyAccessor.Parameter);
    var anyBody = Expression.Call(null, StringMethodCache.Any, Expression.Constant(array), expression);

    return Expression.Lambda<Func<T, bool>>(anyBody);
}

    static Expression MakeStringComparison(Expression left, Comparison comparison, string value, StringComparison stringComparison)
    {
        var valueConstant = Expression.Constant(value, typeof(string));
        var comparisonConstant = Expression.Constant(stringComparison, typeof(StringComparison));
        //TODO: cache methods
        switch (comparison)
        {
            case Comparison.Equal:
                return Expression.Call(StringMethodCache.Equal, left, valueConstant, comparisonConstant);
            case Comparison.NotEqual:
                var notEqualsCall = Expression.Call(StringMethodCache.Equal, left, valueConstant, comparisonConstant);
                return Expression.Not(notEqualsCall);
            case Comparison.StartsWith:
                return Expression.Call(left, StringMethodCache.StartsWith, valueConstant, comparisonConstant);
            case Comparison.EndsWith:
                return Expression.Call(left, StringMethodCache.EndsWith, valueConstant, comparisonConstant);
            case Comparison.Contains:
                var call = Expression.Call(left, StringMethodCache.IndexOf, valueConstant, comparisonConstant);
                return Expression.NotEqual(call, Expression.Constant(-1));
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

    public class C
    {
        static Expression<Func<Foo, bool>> InExpression(string[] array, StringComparison stringComparison)
        {
            return x => Enumerable.Any<string>(array, y => string.Equals(x.Id, y, stringComparison));
        }
    }

    class Foo
    {
        public string Id { get; set; }
    }

}