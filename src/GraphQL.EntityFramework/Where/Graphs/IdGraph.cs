namespace GraphQL.EntityFramework;

public class IdGraph :
    IdGraphType
{
    public IdGraph() =>
        Name = "Id";

    public override bool CanParseLiteral(GraphQLValue value)
    {
        if (value is GraphQLNullValue)
        {
            return false;
        }

        return base.CanParseLiteral(value);
    }

    public override object? ParseLiteral(GraphQLValue value)
    {
        if (value is GraphQLNullValue)
        {
            ThrowLiteralConversionError(value);
        }

        return base.ParseLiteral(value);
    }
}