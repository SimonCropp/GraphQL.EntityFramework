﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GraphQL.EntityFramework.Testing
{
    public class ClientQueryExecutor
    {
        string uri = "graphql";
        Func<object, string> toJson;

        public ClientQueryExecutor(Func<object, string> toJson, string uri = "graphql")
        {
            this.uri = uri;
            this.toJson = toJson;
        }

        public Task<HttpResponseMessage> ExecutePost(HttpClient client, string query, object? variables = null, Action<HttpHeaders>? headerAction = null)
        {
            Guard.AgainstWhiteSpace(nameof(query), query);
            query = CompressQuery(query);
            var body = new
            {
                query,
                variables
            };
            HttpRequestMessage request = new(HttpMethod.Post, uri)
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
            HttpRequestMessage request = new(HttpMethod.Get, getUri);
            headerAction?.Invoke(request.Headers);
            return client.SendAsync(request);
        }

        string ToJson(object? target)
        {
            if (target == null)
            {
                return string.Empty;
            }

            return toJson(target);
        }

        static string CompressQuery(string query)
        {
            return Compress.Query(query);
        }
    }
}