using GraphQL.Common.Request;

public class GraphQLSubscriptionRequest
{
    public string Id { get; set; } = null!;

    /// <summary>The Type of the Request</summary>
    public string Type { get; set; } = null!;

    /// <summary>The Payload of the Request</summary>
    public GraphQLRequest Payload { get; set; } = null!;
}