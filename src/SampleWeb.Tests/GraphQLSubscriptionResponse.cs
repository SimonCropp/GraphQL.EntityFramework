using GraphQL.Common.Response;

public class GraphQLSubscriptionResponse
{
    /// <summary>The Identifier of the Response</summary>
    public string Id { get; set; } = null!;

    /// <summary>The Type of the Response</summary>
    public string Type { get; set; } = null!;

    /// <summary>The Payload of the Response</summary>
    public GraphQLResponse Payload { get; set; } = null!;
}