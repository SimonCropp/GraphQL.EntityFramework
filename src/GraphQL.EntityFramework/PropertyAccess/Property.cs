interface IProperty
{
    Expression Left { get; }
    ParameterExpression SourceParameter { get; }
}

record Property<TInput>(
    Expression Left,
    Expression<Func<TInput, object>> Lambda,
    ParameterExpression SourceParameter,
    Type PropertyType,
    MemberInfo Info,
    MethodInfo? ListContains) :
    IProperty
{
    /// <summary>
    /// Compiled on first use rather than up front. Compiling emits IL, which costs orders of
    /// magnitude more than building the expression tree, and only the in memory paths (list
    /// ordering, and AutoMap) ever invoke the delegate. The queryable paths use <see cref="Left"/>
    /// and <see cref="Lambda"/>, so they should not pay for it.
    /// </summary>
    public Func<TInput, object> Func =>
        // A race here just means two threads compile and one result is discarded, which is
        // harmless: the delegates are equivalent.
        field ??= Lambda.Compile();

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
