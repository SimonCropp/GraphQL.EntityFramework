static class ProjectionAnalyzer
{
    public static IReadOnlySet<string> ExtractPropertyPaths<TEntity, TProjection>(
        Expression<Func<TEntity, TProjection>> projection) =>
        ExtractPropertyPaths((LambdaExpression)projection);

    public static IReadOnlySet<string> ExtractPropertyPaths(LambdaExpression projection)
    {
        var visitor = new PropertyAccessVisitor(projection.Parameters);
        visitor.Visit(projection);
        return visitor.AccessedProperties;
    }

    sealed class PropertyAccessVisitor(IReadOnlyCollection<ParameterExpression> parameters) : ExpressionVisitor
    {
        public HashSet<string> AccessedProperties { get; } = [with(StringComparer.OrdinalIgnoreCase)];

        protected override Expression VisitMember(MemberExpression node)
        {
            var path = PathFromParameter(node);
            if (path is not null)
            {
                AccessedProperties.Add(path);
            }

            return base.VisitMember(node);
        }

        /// <summary>
        /// A collection read through LINQ, such as
        /// <c>_.Children.Where(child => child.Active).Select(child => child.Name)</c>, reads
        /// properties of its items inside the lambdas, off a parameter that is not the
        /// projection's own. Only the collection was recorded, and a navigation with nothing
        /// recorded under it is loaded whole, with every column of every item. The properties are
        /// recorded under the collection instead, so only they are loaded.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            AddItemProperties(node);
            return base.VisitMethodCall(node);
        }

        void AddItemProperties(MethodCallExpression node)
        {
            if (!IsLinq(node))
            {
                return;
            }

            // The calls from this one down to the collection they read
            var lambdas = new List<LambdaExpression>();
            Expression current = node;
            while (current is MethodCallExpression call &&
                   IsLinq(call))
            {
                foreach (var argument in call.Arguments.Skip(1))
                {
                    if (StripQuote(argument) is LambdaExpression lambda)
                    {
                        lambdas.Add(lambda);
                    }
                }

                current = StripConvert(call.Arguments[0]);
            }

            if (current is not MemberExpression collection ||
                !TryGetItemType(collection.Type, out var itemType))
            {
                return;
            }

            var path = PathFromParameter(collection);
            if (path is null)
            {
                return;
            }

            // The items themselves come out, as from a Where with no Select after it, so
            // whatever uses them can read any property
            if (node.Type.IsAssignableTo(itemType) ||
                (TryGetItemType(node.Type, out var resultItemType) &&
                 resultItemType.IsAssignableTo(itemType)))
            {
                return;
            }

            var reads = new ItemReadVisitor(itemType);
            foreach (var lambda in lambdas)
            {
                reads.Visit(lambda.Body);
            }

            if (reads.ReadsWholeItem)
            {
                return;
            }

            foreach (var name in reads.Properties)
            {
                AccessedProperties.Add($"{path}.{name}");
            }
        }

        string? PathFromParameter(MemberExpression node)
        {
            var path = new List<string>();
            Expression? current = node;

            while (current is MemberExpression or UnaryExpression)
            {
                if (current is MemberExpression memberExpr)
                {
                    path.Insert(0, memberExpr.Member.Name);
                    current = memberExpr.Expression;
                }
                else if (current is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.TypeAs } unary)
                {
                    current = unary.Operand;
                }
                else
                {
                    break;
                }
            }

            if (current is ParameterExpression param &&
                parameters.Contains(param) &&
                path.Count > 0)
            {
                return string.Join('.', path);
            }

            return null;
        }
    }

    /// <summary>
    /// What the lambdas of a LINQ chain read from the items of the collection. A scalar read
    /// directly off an item is a property to load. Anything else, such as the item itself, a
    /// navigation of it, or the item passed to a method, means the whole item is needed.
    /// </summary>
    sealed class ItemReadVisitor(Type itemType) : ExpressionVisitor
    {
        public HashSet<string> Properties { get; } = [with(StringComparer.OrdinalIgnoreCase)];
        public bool ReadsWholeItem { get; private set; }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression is not null &&
                IsItem(StripConvert(node.Expression)))
            {
                if (IsScalar(node.Type))
                {
                    Properties.Add(node.Member.Name);
                }
                else
                {
                    ReadsWholeItem = true;
                }

                return node;
            }

            return base.VisitMember(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (IsItem(node))
            {
                ReadsWholeItem = true;
            }

            return node;
        }

        // The parameters are the items coming in, not a use of them
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            Visit(node.Body);
            return node;
        }

        bool IsItem(Expression expression) =>
            expression is ParameterExpression parameter &&
            parameter.Type.IsAssignableTo(itemType);

        static bool IsScalar(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return type.IsValueType ||
                   type == typeof(string) ||
                   type == typeof(byte[]);
        }
    }

    static bool IsLinq(MethodCallExpression call) =>
        call.Arguments.Count > 0 &&
        (call.Method.DeclaringType == typeof(Enumerable) ||
         call.Method.DeclaringType == typeof(Queryable));

    static Expression StripQuote(Expression expression)
    {
        if (expression is UnaryExpression { NodeType: ExpressionType.Quote } quote)
        {
            return quote.Operand;
        }

        return expression;
    }

    static Expression StripConvert(Expression expression)
    {
        while (expression is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.TypeAs } unary)
        {
            expression = unary.Operand;
        }

        return expression;
    }

    // The entity a collection navigation holds. A string is a sequence of chars, and a byte
    // array of bytes, but neither is a collection of items.
    static bool TryGetItemType(Type type, [NotNullWhen(true)] out Type? itemType)
    {
        itemType = null;
        if (type == typeof(string))
        {
            return false;
        }

        foreach (var candidate in type.GetInterfaces().Append(type))
        {
            if (candidate.IsGenericType &&
                candidate.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var argument = candidate.GetGenericArguments()[0];
                if (argument.IsClass &&
                    argument != typeof(string))
                {
                    itemType = argument;
                    return true;
                }
            }
        }

        return false;
    }
}
