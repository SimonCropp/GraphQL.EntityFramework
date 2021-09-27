using System.Linq.Expressions;

record Property<TInput>(
    Expression Left,
    Expression<Func<TInput, object>> Lambda,
    ParameterExpression SourceParameter,
    Func<TInput, object> Func,
    Type PropertyType,
    MemberInfo Info,
    MethodInfo? ListContainsMethod);
