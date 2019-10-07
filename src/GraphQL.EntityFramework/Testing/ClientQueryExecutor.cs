using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GraphQL.EntityFramework.Testing
{
    public static class ClientQueryExecutor
    {
        static string uri = "graphql";

        public static void SetQueryUri(string uri)
        {
            Guard.AgainstNullWhiteSpace(nameof(uri), uri);
            ClientQueryExecutor.uri = uri;
        }

        public static Task<HttpResponseMessage> ExecutePost(HttpClient client, string query, object? variables = null, Action<HttpHeaders>? headerAction = null)
        {
            Guard.AgainstNull(nameof(client), client);
            Guard.AgainstNullWhiteSpace(nameof(query), query);
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

        public static Task<HttpResponseMessage> ExecuteGet(HttpClient client, string query, object? variables = null, Action<HttpHeaders>? headerAction = null)
        {
            Guard.AgainstNull(nameof(client), client);
            Guard.AgainstNullWhiteSpace(nameof(query), query);
            var compressed = CompressQuery(query);
            var variablesString = ToJson(variables);
            var getUri = $"{uri}?query={compressed}&variables={variablesString}";
            var request = new HttpRequestMessage(HttpMethod.Get, getUri);
            headerAction?.Invoke(request.Headers);
            return client.SendAsync(request);
        }

        static string ToJson(object? target)
        {
            if (target == null)
            {
                return string.Empty;
            }

            return JsonConvert.SerializeObject(target);
        }

        static string CompressQuery(string query)
        {
            return Compress.Query(query);
        }
    }
}