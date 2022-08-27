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