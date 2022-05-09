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
}