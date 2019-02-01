using System;
using System.Linq.Expressions;

class Property<TInput>
{
    public Func<TInput, object> Func;
    public ParameterExpression SourceParameter;
    public Expression Left;
    public Type PropertyType;
    public Expression<Func<TInput, object>> Lambda;
}