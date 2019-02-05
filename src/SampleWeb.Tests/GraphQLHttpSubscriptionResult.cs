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
    private readonly byte[] buffer = new byte[1048576];

    private readonly Uri _webSocketUri;

    private readonly GraphQLRequest _graphQLRequest;

    private readonly WebSocketClient _clientWebSocket;

    public event Action<GraphQLResponse> OnReceive;

    public GraphQLResponse LastResponse { get; private set; }

    internal GraphQLHttpSubscriptionResult(Uri webSocketUri, GraphQLRequest graphQLRequest, WebSocketClient clientWebSocket)
    {
        _webSocketUri = webSocketUri;

        _graphQLRequest = graphQLRequest;

        _clientWebSocket = clientWebSocket;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        var clientSocket = await _clientWebSocket.ConnectAsync(_webSocketUri, cancellationToken);
        if (clientSocket.State != WebSocketState.Open)
        {
            return;
        }

        var arraySegment = new ArraySegment<byte>(buffer);
        var graphQlSubscriptionRequest = new GraphQLSubscriptionRequest
        {
            Id = "1",
            Type = "start",
            Payload = _graphQLRequest
        };

        var jsonRequest = JsonConvert.SerializeObject(graphQlSubscriptionRequest);

        var payload = new ArraySegment<byte>(Encoding.UTF8.GetBytes(jsonRequest));

        await clientSocket.SendAsync(payload, messageType: WebSocketMessageType.Text, endOfMessage: true, cancellationToken: cancellationToken);

        try
        {
            while (clientSocket.State == WebSocketState.Open)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var webSocketReceiveResult = await clientSocket.ReceiveAsync(arraySegment, cancellationToken);

                var response = Encoding.UTF8.GetString(arraySegment.Array, 0, webSocketReceiveResult.Count);

                var subscriptionResponse = JsonConvert.DeserializeObject<GraphQLSubscriptionResponse>(response);
                if (subscriptionResponse != null)
                {
                    LastResponse = subscriptionResponse.Payload;
                    OnReceive?.Invoke(subscriptionResponse.Payload);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}