using GraphQL.Common.Response;

public class GraphQLSubscriptionResponse
{
    /// <summary>The Identifier of the Response</summary>
    public string Id { get; set; }

    /// <summary>The Type of the Response</summary>
    public string Type { get; set; }

    /// <summary>The Payload of the Response</summary>
    public GraphQLResponse Payload { get; set; }
}