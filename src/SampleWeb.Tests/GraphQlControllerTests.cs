using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GraphQL.EntityFramework.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json.Linq;
using Xunit;
#region GraphQlControllerTests
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
        var response = await ClientQueryExecutor.ExecuteGet(client, query);
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

        var response = await ClientQueryExecutor.ExecuteGet(client, query, variables);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();
        Assert.Equal("{\"data\":{\"companies\":[{\"id\":1}]}}", result);
    }

    [Fact]
    public async Task Get_company_paging()
    {
        var after = 1;
        var query = @"
query {
  companiesConnection(first:2, after:""" + after + @""") {
    edges {
      cursor
      node {
        id
      }
    }
    pageInfo {
      endCursor
      hasNextPage
    }
  }
}";
        var response = await ClientQueryExecutor.ExecuteGet(client, query);
        response.EnsureSuccessStatusCode();
        var result = JObject.Parse(await response.Content.ReadAsStringAsync());

        var firstOfNextPage = result.SelectToken("..data..companiesConnection..edges[0].cursor")
            .Value<string>();
        Assert.NotEqual(after.ToString(), firstOfNextPage);
    }

    [Fact]
    public async Task Get_null_query()
    {
        var response = await ClientQueryExecutor.ExecuteGet(client);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadAsStringAsync();
        Assert.Contains("A query is required.", result);
    }

    [Fact]
    public async Task Post()
    {
        var query = @"
{
  companies
  {
    id
  }
}";
        var response = await ClientQueryExecutor.ExecutePost(client, query);
        var result = await response.Content.ReadAsStringAsync();
        Assert.Equal("{\"data\":{\"companies\":[{\"id\":1},{\"id\":4},{\"id\":6},{\"id\":7}]}}", result);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Post_variable()
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
        var response = await ClientQueryExecutor.ExecutePost(client, query, variables);
        var result = await response.Content.ReadAsStringAsync();
        Assert.Equal("{\"data\":{\"companies\":[{\"id\":1}]}}", result);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Post_null_query()
    {
        var response = await ClientQueryExecutor.ExecutePost(client);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadAsStringAsync();
        Assert.Contains("A query is required.", result);
    }

    static TestServer GetTestServer()
    {
        var hostBuilder = new WebHostBuilder();
        hostBuilder.UseStartup<Startup>();
        return new TestServer(hostBuilder);
    }
}
#endregion