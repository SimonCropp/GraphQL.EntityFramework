using System.Text.RegularExpressions;

/// <summary>
/// The where expressions are built for sql, where a string comparison follows the database
/// collation, which is case insensitive by default on sql server. Evaluated in memory the same
/// expressions were ordinal, so a where gave different results on a navigation list than on a
/// root field. This rewrites the string calls to ignore case, and replaces EF.Functions.Like,
/// which throws outside a query, with an in memory pattern match.
/// </summary>
class InMemoryStringComparison :
    ExpressionVisitor
{
    static InMemoryStringComparison instance = new();

    static ConstantExpression ignoreCase = Expression.Constant(StringComparison.OrdinalIgnoreCase);

    static MethodInfo equals = typeof(string)
        .GetMethod(nameof(string.Equals), [typeof(string), typeof(string), typeof(StringComparison)])!;

    static MethodInfo startsWith = typeof(string)
        .GetMethod(nameof(string.StartsWith), [typeof(string), typeof(StringComparison)])!;

    static MethodInfo endsWith = typeof(string)
        .GetMethod(nameof(string.EndsWith), [typeof(string), typeof(StringComparison)])!;

    static MethodInfo indexOf = typeof(string)
        .GetMethod(nameof(string.IndexOf), [typeof(string), typeof(StringComparison)])!;

    static MethodInfo like = typeof(InMemoryStringComparison)
        .GetMethod(nameof(Like), BindingFlags.Static | BindingFlags.NonPublic)!;

    public static Expression<Func<T, bool>> Rewrite<T>(Expression<Func<T, bool>> predicate) =>
        (Expression<Func<T, bool>>) instance.Visit(predicate);

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        var method = node.Method;

        if (method == ReflectionCache.StringEqual)
        {
            return Expression.Call(equals, Visit(node.Arguments[0]), Visit(node.Arguments[1]), ignoreCase);
        }

        if (method == ReflectionCache.StringStartsWith)
        {
            return Expression.Call(Visit(node.Object)!, startsWith, Visit(node.Arguments[0]), ignoreCase);
        }

        if (method == ReflectionCache.StringEndsWith)
        {
            return Expression.Call(Visit(node.Object)!, endsWith, Visit(node.Arguments[0]), ignoreCase);
        }

        if (method == ReflectionCache.StringIndexOf)
        {
            return Expression.Call(Visit(node.Object)!, indexOf, Visit(node.Arguments[0]), ignoreCase);
        }

        if (method == ReflectionCache.StringLike)
        {
            return Expression.Call(like, Visit(node.Arguments[1]), Visit(node.Arguments[2]));
        }

        return base.VisitMethodCall(node);
    }

    static ConcurrentDictionary<string, Regex> patterns = new();

    // Patterns arrive from the client, so the cache is bounded the same way as the property cache
    const int maxCachedPatterns = 1000;

    static bool Like(string? input, string? pattern)
    {
        if (input is null || pattern is null)
        {
            return false;
        }

        if (!patterns.TryGetValue(pattern, out var regex))
        {
            regex = ToRegex(pattern);
            if (patterns.Count >= maxCachedPatterns)
            {
                patterns.Clear();
            }

            patterns.TryAdd(pattern, regex);
        }

        return regex.IsMatch(input);
    }

    /// <summary>
    /// A sql like pattern as a regex: % is any run, _ is any single character, and a character
    /// class such as [abc], [a-z] or [^abc] has the same shape in both. Everything else is literal.
    /// Non backtracking, since a client supplied pattern must not be able to stall the server.
    /// </summary>
    static Regex ToRegex(string pattern)
    {
        var builder = new StringBuilder("^");
        var index = 0;
        while (index < pattern.Length)
        {
            var current = pattern[index];
            switch (current)
            {
                case '%':
                    builder.Append(".*");
                    break;
                case '_':
                    builder.Append('.');
                    break;
                case '[':
                    var end = pattern.IndexOf(']', index + 1);
                    if (end < 0)
                    {
                        builder.Append(Regex.Escape("["));
                        break;
                    }

                    builder
                        .Append('[')
                        .Append(pattern.AsSpan(index + 1, end - index - 1).ToString().Replace("\\", "\\\\"))
                        .Append(']');
                    index = end;
                    break;
                default:
                    builder.Append(Regex.Escape(current.ToString()));
                    break;
            }

            index++;
        }

        builder.Append('$');
        return new(
            builder.ToString(),
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);
    }
}
