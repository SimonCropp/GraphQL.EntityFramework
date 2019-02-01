using System;
using System.Linq.Expressions;

class PropertyExpression
{
    public ParameterExpression SourceParameter;
    public Expression Left;
    public Type PropertyType;
}


class Property<TInput>
{
    public Func<TInput, object> Func;
    public ParameterExpression SourceParameter;
    public Expression Left;
    public Type PropertyType;
}