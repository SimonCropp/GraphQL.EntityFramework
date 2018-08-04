using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;

public class GraphQlControllerTests
{
    static HttpClient client;

    static GraphQlControllerTests()
    {
        var server = GetTestServer();
        client = server.CreateClient();
    }

    [Fact]
    public async Task Get()
    {
        var query = @"
{
  companies
  {
    id
  }
}";
        var response = await ExecuteGet(query);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();
        Assert.Equal("{\"data\":{\"companies\":[{\"id\":1},{\"id\":4},{\"id\":6},{\"id\":7}]}}", result);
    }

    [Fact]
    public async Task Get_variable()
    {
        var query = @"
query ($id: String!)
{
  companies(ids:[$id])
  {
    id
  }
}";
        var variables = new
        {
            id = "1"
        };

        var response = await ExecuteGet(query, variables);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();
        Assert.Equal("{\"data\":{\"companies\":[{\"id\":1}]}}", result);
    }

    [Fact]
    public async Task Get_null_query()
    {
        var response = await ExecuteGet();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadAsStringAsync();
        Assert.Contains("GraphQL.ExecutionError: A query is required.", result);
    }

    static Task<HttpResponseMessage> ExecuteGet(string query = null, object variables = null)
    {
        var compressed = Compress(query);
        var variablesString = ToJson(variables);
        var uri = $"graphql?query={compressed}&variables={variablesString}";
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        return client.SendAsync(request);
    }

    [Fact]
    public async Task Post()
    {
        var response = await ExecutePost("{companies{id}}");
        var result = await response.Content.ReadAsStringAsync();
        Assert.Equal("{\"data\":{\"companies\":[{\"id\":1},{\"id\":4},{\"id\":6},{\"id\":7}]}}", result);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Post_variable()
    {
        var variables = new
        {
            id = "1"
        };
        var response = await ExecutePost("query ($id: String!){companies(ids:[$id]){id}}", variables);
        var result = await response.Content.ReadAsStringAsync();
        Assert.Equal("{\"data\":{\"companies\":[{\"id\":1}]}}", result);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Post_null_query()
    {
        var response = await ExecutePost();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadAsStringAsync();
        Assert.Contains("GraphQL.ExecutionError: A query is required.", result);
    }

    static Task<HttpResponseMessage> ExecutePost(string query = null, object variables = null)
    {
        query = Compress(query);
        var body = new
        {
            query,
            variables
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "graphql")
        {
            Content = new StringContent(ToJson(body), Encoding.UTF8, "application/json")
        };
        return client.SendAsync(request);
    }

    static TestServer GetTestServer()
    {
        var hostBuilder = new WebHostBuilder();
        hostBuilder.UseStartup<Startup>();
        return new TestServer(hostBuilder);
    }

    static string ToJson(object target)
    {
        if (target == null)
        {
            return "";
        }
        return JsonConvert.SerializeObject(target);
    }

    static string Compress(string query)
    {
        if (query == null)
        {
            return "";
        }
        return GraphQL.EntityFramework.Compress.Query(query);
    }
}