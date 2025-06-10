record Property<TInput>(
    Expression Left,
    Expression<Func<TInput, object>> Lambda,
    ParameterExpression SourceParameter,
    Func<TInput, object> Func,
    Type PropertyType,
    MemberInfo Info,
    MethodInfo? ListContains)
{
    public MethodInfo SafeListContains
    {
        get
        {
            if (ListContains is null)
            {
                throw new($"No ListContains found for {typeof(TInput).Name}");
            }

            return ListContains;
        }
    }
}
