using System.Net.Http;
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

        public static Task<HttpResponseMessage> ExecutePost(HttpClient client, string query = null, object variables = null)
        {
            Guard.AgainstNull(nameof(client), client);
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
            return client.SendAsync(request);
        }

        public static Task<HttpResponseMessage> ExecuteGet(HttpClient client, string query = null, object variables = null)
        {
            Guard.AgainstNull(nameof(client), client);
            var compressed = CompressQuery(query);
            var variablesString = ToJson(variables);
            var getUri = $"{uri}?query={compressed}&variables={variablesString}";
            var request = new HttpRequestMessage(HttpMethod.Get, getUri);
            return client.SendAsync(request);
        }

        static string ToJson(object target)
        {
            if (target == null)
            {
                return "";
            }

            return JsonConvert.SerializeObject(target);
        }

        static string CompressQuery(string query)
        {
            if (query == null)
            {
                return "";
            }

            return Compress.Query(query);
        }
    }
}