namespace GraphQL.EntityFramework;

/// <summary>
/// The comparisons for a value of <typeparamref name="TValue"/>. Each field is a comparison, and
/// the value is typed, so GraphQL.NET coerces it and no string parsing happens in the library.
/// </summary>
public class ComparisonGraph<TValue> :
    InputObjectGraphType
{
    public ComparisonGraph()
    {
        var type = typeof(TValue);
        Name = GraphNames.Comparison(type);
        var valueType = type.GetGraphTypeFromType(true, TypeMappingMode.InputType);
        var listType = typeof(ListGraphType<>).MakeGenericType(valueType);
        Add("equal", valueType);
        Add("notEqual", valueType);
        Add("in", listType);
        if (type == typeof(string))
        {
            Add("startsWith", valueType);
            Add("endsWith", valueType);
            Add("contains", valueType);
            Add("like", valueType);
            return;
        }

        if (type == typeof(bool) ||
            type.IsEnum)
        {
            return;
        }

        Add("greaterThan", valueType);
        Add("greaterThanOrEqual", valueType);
        Add("lessThan", valueType);
        Add("lessThanOrEqual", valueType);
    }

    void Add(string name, Type type) =>
        AddField(new()
        {
            Name = name,
            Type = type
        });

    public override object ParseDictionary(IDictionary<string, object?> value) =>
        value;
}
