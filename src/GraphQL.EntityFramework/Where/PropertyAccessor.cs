using System;
using System.Linq.Expressions;

class PropertyAccessor
{
    public ParameterExpression SourceParameter;
    public Expression Left;
    public Type Type;
}