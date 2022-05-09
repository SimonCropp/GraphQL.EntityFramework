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
}
