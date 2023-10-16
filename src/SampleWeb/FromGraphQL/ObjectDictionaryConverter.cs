/// <summary>
/// A custom JsonConverter for reading a dictionary of objects of their real underlying type.
/// Doesn't support write.
/// </summary>
class ObjectDictionaryConverter : JsonConverter<Dictionary<string, object?>>
{
    public override void Write(Utf8JsonWriter writer, Dictionary<string, object?> value, JsonSerializerOptions options)
        => throw new NotImplementedException(
            "This converter currently is only intended to be used to read a JSON object into a strongly-typed representation.");

    public override Dictionary<string, object?> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => ReadDictionary(ref reader);

    static Dictionary<string, object?> ReadDictionary(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        var result = new Dictionary<string, object?>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            var key = reader.GetString()!;

            // move to property value
            if (!reader.Read())
            {
                throw new JsonException();
            }

            result.Add(key, ReadValue(ref reader));
        }

        return result;
    }

    static object? ReadValue(ref Utf8JsonReader reader)
        => reader.TokenType switch
        {
            JsonTokenType.StartArray => ReadArray(ref reader),
            JsonTokenType.StartObject => ReadDictionary(ref reader),
            JsonTokenType.Number => ReadNumber(ref reader),
            JsonTokenType.True => BoolBox.True,
            JsonTokenType.False => BoolBox.False,
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Null => null,
            JsonTokenType.None => null,
            _ => throw new InvalidOperationException($"Unexpected token type: {reader.TokenType}")
        };

    static List<object?> ReadArray(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException();
        }

        var result = new List<object?>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                break;
            }

            result.Add(ReadValue(ref reader));
        }

        return result;
    }

    static object ReadNumber(ref Utf8JsonReader reader)
    {
        if (reader.TryGetInt32(out var i))
        {
            return i;
        }

        if (reader.TryGetInt64(out var l))
        {
            return l;
        }

        if (reader.TryGetDouble(out var d))
        {
            return d;
        }

        if (reader.TryGetDecimal(out var dm))
        {
            return dm;
        }

        var span = reader.HasValueSequence ?
            reader.ValueSequence.ToArray() :
            reader.ValueSpan;
        throw new NotImplementedException($"Unexpected Number value. Raw text was: {Encoding.UTF8.GetString(span)}");
    }
}