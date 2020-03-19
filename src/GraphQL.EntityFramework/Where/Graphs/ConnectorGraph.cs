using System;
using GraphQL.Language.AST;
using GraphQL.Types;
using GraphQL.EntityFramework;

class ConnectorGraph :
    EnumerationGraphType<Connector>
{
    public ConnectorGraph()
    {
        AddValue("and", null, Connector.And);
        AddValue("or", null, Connector.Or);
    }

    public override object? ParseLiteral(IValue value)
    {
        var literal = base.ParseLiteral(value);
        if (literal != null)
        {
            return literal;
        }

        if (value is StringValue str)
        {
            var strValue = str.Value;
            if (Enum.TryParse(strValue, true, out Connector comparison))
            {
                return comparison;
            }
        }

        return null;
    }
}
