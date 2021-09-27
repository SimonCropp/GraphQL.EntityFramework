using GraphQL.Language.AST;
using GraphQL.Types;
using GraphQL.EntityFramework;

class ConnectorGraph :
    EnumerationGraphType
{
    public ConnectorGraph()
    {
        Name = nameof(Connector);
        AddValue("and", null, Connector.And);
        AddValue("or", null, Connector.Or);
    }

    public override bool CanParseLiteral(IValue value)
    {
        value = value.TryToEnumValue();
        return base.CanParseLiteral(value);
    }

    public override object? ParseLiteral(IValue value)
    {
        var literal = base.ParseLiteral(value.TryToEnumValue());

        if (literal is not null)
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
