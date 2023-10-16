using System.Net.Http.Headers;

namespace GraphQL.EntityFramework.Testing;

public class ClientQueryExecutor(Func<object, string> json, string uri = "graphql")
{
    public Task<HttpResponseMessage> ExecutePost(HttpClient client, string query, object? variables = null, Action<HttpHeaders>? headerAction = null)
    {
        Guard.AgainstWhiteSpace(nameof(query), query);
        query = CompressQuery(query);
        var body = new
        {
            query,
            variables
        };
        var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = new StringContent(ToJson(body), Encoding.UTF8, "application/json")
        };
        headerAction?.Invoke(request.Headers);
        return client.SendAsync(request);
    }

    public Task<HttpResponseMessage> ExecuteGet(HttpClient client, string query, object? variables = null, Action<HttpHeaders>? headerAction = null)
    {
        Guard.AgainstWhiteSpace(nameof(query), query);
        var compressed = CompressQuery(query);
        var variablesString = ToJson(variables);
        var getUri = $"{uri}?query={compressed}&variables={variablesString}";
        var request = new HttpRequestMessage(HttpMethod.Get, getUri);
        headerAction?.Invoke(request.Headers);
        return client.SendAsync(request);
    }

    string ToJson(object? target)
    {
        if (target is null)
        {
            return string.Empty;
        }

        return json(target);
    }

    static string CompressQuery(string query) =>
        Compress.Query(query);
}