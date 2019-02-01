using System;
using System.Linq.Expressions;
using System.Reflection;

class Property<TInput>
{
    public Func<TInput, object> Func;
    public ParameterExpression SourceParameter;
    public Expression Left;
    public Type PropertyType;
    public Expression<Func<TInput, object>> Lambda;
    public MethodInfo ListContainsMethod;
}