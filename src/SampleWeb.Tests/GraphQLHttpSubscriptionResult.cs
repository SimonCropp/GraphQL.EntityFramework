using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Common.Request;
using GraphQL.Common.Response;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;

public class GraphQLHttpSubscriptionResult
{
    byte[] buffer = new byte[1048576];

    Uri webSocketUri;

    GraphQLRequest graphQLRequest;

    WebSocketClient clientWebSocket;

    Action<GraphQLResponse> onReceive;

    public GraphQLResponse LastResponse { get; private set; } = null!;

    internal GraphQLHttpSubscriptionResult(Uri webSocketUri, GraphQLRequest graphQLRequest, WebSocketClient clientWebSocket, Action<GraphQLResponse> onReceive)
    {
        this.webSocketUri = webSocketUri;
        this.graphQLRequest = graphQLRequest;
        this.clientWebSocket = clientWebSocket;
        this.onReceive = onReceive;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        var clientSocket = await clientWebSocket.ConnectAsync(webSocketUri, cancellationToken);
        if (clientSocket.State != WebSocketState.Open)
        {
            return;
        }

        var arraySegment = new ArraySegment<byte>(buffer);
        var graphQlSubscriptionRequest = new GraphQLSubscriptionRequest
        {
            Id = "1",
            Type = "start",
            Payload = graphQLRequest
        };

        var jsonRequest = JsonConvert.SerializeObject(graphQlSubscriptionRequest);

        var payload = new ArraySegment<byte>(Encoding.UTF8.GetBytes(jsonRequest));

        await clientSocket.SendAsync(
            payload,
            messageType: WebSocketMessageType.Text,
            endOfMessage: true,
            cancellationToken: cancellationToken);

        try
        {
            while (clientSocket.State == WebSocketState.Open)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var webSocketReceiveResult = await clientSocket.ReceiveAsync(arraySegment, cancellationToken);

                var response = Encoding.UTF8.GetString(arraySegment.Array!, 0, webSocketReceiveResult.Count);

                var subscriptionResponse = JsonConvert.DeserializeObject<GraphQLSubscriptionResponse>(response);
                if (subscriptionResponse != null)
                {
                    LastResponse = subscriptionResponse.Payload;
                    onReceive.Invoke(subscriptionResponse.Payload);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}