using System;
using System.Linq.Expressions;
using System.Reflection;

class Property<TInput>
{
    public readonly Func<TInput, object> Func;
    public readonly ParameterExpression SourceParameter;
    public readonly Expression Left;
    public readonly Type PropertyType;
    public readonly Expression<Func<TInput, object>> Lambda;
    public readonly MethodInfo? ListContainsMethod;

    public Property(Expression left, Expression<Func<TInput, object>> lambda, ParameterExpression sourceParameter, Func<TInput, object> func, Type propertyType, MethodInfo? listContainsMethod)
    {
        Left = left;
        Lambda = lambda;
        SourceParameter = sourceParameter;
        Func = func;
        PropertyType = propertyType;
        ListContainsMethod = listContainsMethod;
    }
}