using GraphQL.Types;
using GraphQL.EntityFramework;

class ConnectorGraph :
    EnumerationGraphType
{
    public ConnectorGraph()
    {
        Name = nameof(Connector);
        Add("and", Connector.And);
        Add("or", Connector.Or);
    }

    //TODO:
    // public override object? ParseLiteral(IValue value)
    // {
    //     var literal = base.ParseLiteral(value);
    //
    //     if (literal is not null)
    //     {
    //         return literal;
    //     }
    //
    //     if (value is StringValue str)
    //     {
    //         var strValue = str.Value;
    //         if (Enum.TryParse(strValue, true, out Connector comparison))
    //         {
    //             return comparison;
    //         }
    //     }
    //
    //     return null;
    // }
}
