using System;

class PropertyFunc<TInput>
{
    public Func<TInput, object> Func;
    public Type PropertyType;
}