namespace GraphQL.EntityFramework;

/// <summary>
/// The where input type for <typeparamref name="TEntity"/>. One field per mapped property,
/// typed by its comparisons, one per navigation, and <c>and</c>, <c>or</c> and <c>not</c> to
/// compose them. Sibling fields are and'ed. The parsed value is a <see cref="WhereExpression"/>
/// tree, so the predicate builder is unchanged.
/// </summary>
public class WhereGraph<TEntity> :
    InputObjectGraphType,
    IWhereGraph
{
    const string memberKey = "EfMember";

    static FrozenDictionary<string, Comparison> comparisons = FrozenDictionary.ToFrozenDictionary(
    [
        KeyValuePair.Create("equal", Comparison.Equal),
        KeyValuePair.Create("notEqual", Comparison.NotEqual),
        KeyValuePair.Create("in", Comparison.In),
        KeyValuePair.Create("startsWith", Comparison.StartsWith),
        KeyValuePair.Create("endsWith", Comparison.EndsWith),
        KeyValuePair.Create("contains", Comparison.Contains),
        KeyValuePair.Create("like", Comparison.Like),
        KeyValuePair.Create("greaterThan", Comparison.GreaterThan),
        KeyValuePair.Create("greaterThanOrEqual", Comparison.GreaterThanOrEqual),
        KeyValuePair.Create("lessThan", Comparison.LessThan),
        KeyValuePair.Create("lessThanOrEqual", Comparison.LessThanOrEqual),
    ]);

    static FrozenDictionary<string, Quantifier> quantifiers = FrozenDictionary.ToFrozenDictionary(
    [
        KeyValuePair.Create("any", Quantifier.Any),
        KeyValuePair.Create("all", Quantifier.All),
        KeyValuePair.Create("none", Quantifier.None),
    ]);

    public WhereGraph(IEnumerable<IEfGraphQLService> services)
    {
        var type = typeof(TEntity);
        Name = GraphNames.Where(type);
        var self = typeof(WhereGraph<TEntity>);
        var selfList = typeof(ListGraphType<>).MakeGenericType(typeof(NonNullGraphType<>).MakeGenericType(self));
        AddField(new()
        {
            Name = "and",
            Type = selfList
        });
        AddField(new()
        {
            Name = "or",
            Type = selfList
        });
        AddField(new()
        {
            Name = "not",
            Type = self
        });

        foreach (var member in EntityShape.Members(type, services))
        {
            var fieldType = member.Kind switch
            {
                MemberKind.Scalar => typeof(ComparisonGraph<>).MakeGenericType(member.Type),
                MemberKind.Reference => typeof(WhereGraph<>).MakeGenericType(member.Type),
                MemberKind.Collection => typeof(CollectionWhereGraph<>).MakeGenericType(member.Type),
                _ => throw new($"Unknown member kind {member.Kind}")
            };
            var field = new FieldType
            {
                Name = member.Name,
                Type = fieldType
            };
            field.Metadata[memberKey] = member;
            AddField(field);
        }
    }

    public override object ParseDictionary(IDictionary<string, object?> value)
    {
        var expressions = new List<WhereExpression>();
        foreach (var (key, raw) in value)
        {
            if (raw is null)
            {
                continue;
            }

            switch (key)
            {
                case "and":
                    AddGroup(expressions, raw, Connector.And);
                    continue;
                case "or":
                    AddGroup(expressions, raw, Connector.Or);
                    continue;
                case "not":
                    var negated = (WhereExpression) raw;
                    if (!negated.IsEmpty)
                    {
                        negated.Negate = true;
                        expressions.Add(negated);
                    }

                    continue;
            }

            var member = (EntityMember) GetField(key)!.Metadata[memberKey]!;
            switch (member.Kind)
            {
                case MemberKind.Scalar:
                    AddComparisons(expressions, member.Name, (IDictionary<string, object?>) raw);
                    break;
                case MemberKind.Reference:
                    var nested = (WhereExpression) raw;
                    if (!nested.IsEmpty)
                    {
                        Prefix(nested, member.Name);
                        expressions.Add(nested);
                    }

                    break;
                case MemberKind.Collection:
                    foreach (var (quantifierName, items) in (IDictionary<string, object?>) raw)
                    {
                        if (items is null)
                        {
                            continue;
                        }

                        expressions.Add(new()
                        {
                            Path = member.Name,
                            Quantifier = quantifiers[quantifierName],
                            GroupedExpressions = ((WhereExpression) items).GroupedExpressions ?? []
                        });
                    }

                    break;
            }
        }

        return new WhereExpression
        {
            GroupedExpressions = expressions.ToArray()
        };
    }

    static void AddGroup(List<WhereExpression> expressions, object raw, Connector connector)
    {
        var items = ((IEnumerable) raw)
            .Cast<WhereExpression>()
            .Where(_ => !_.IsEmpty)
            .ToArray();
        if (items.Length == 0)
        {
            return;
        }

        // The connector on an expression joins it to the next one
        for (var index = 0; index < items.Length - 1; index++)
        {
            items[index].Connector = connector;
        }

        expressions.Add(new()
        {
            GroupedExpressions = items
        });
    }

    static void AddComparisons(List<WhereExpression> expressions, string path, IDictionary<string, object?> raw)
    {
        foreach (var (name, value) in raw)
        {
            var comparison = comparisons[name];
            object?[]? values;
            if (comparison == Comparison.In)
            {
                if (value is null)
                {
                    continue;
                }

                values = ((IEnumerable) value)
                    .Cast<object?>()
                    .ToArray();
            }
            else
            {
                values = value is null ? null : [value];
            }

            expressions.Add(new()
            {
                Path = path,
                Comparison = comparison,
                Value = values
            });
        }
    }

    /// <summary>
    /// A nested where is built against the navigation's type, so its paths are extended with the
    /// navigation. A collection node's items are built against the item type and stay as they are.
    /// </summary>
    static void Prefix(WhereExpression expression, string name)
    {
        if (expression.Quantifier is null &&
            expression.GroupedExpressions is { Length: > 0 })
        {
            foreach (var grouped in expression.GroupedExpressions)
            {
                Prefix(grouped, name);
            }

            return;
        }

        expression.Path = $"{name}.{expression.Path}";
    }
}
