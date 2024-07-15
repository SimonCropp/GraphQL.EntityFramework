using System.Net;
using Newtonsoft.Json;
// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

#region GraphQlControllerTests

public class GraphQlControllerTests
{
    static HttpClient client;
    static ClientQueryExecutor clientQueryExecutor;
    static WebSocketClient webSocket;

    static GraphQlControllerTests()
    {
        var server = GetTestServer();
        client = server.CreateClient();
        webSocket = server.CreateWebSocketClient();
        webSocket.ConfigureRequest =
            request =>
            {
                var headers = request.Headers;
                headers.SecWebSocketProtocol = "graphql-ws";
            };
        clientQueryExecutor = new(JsonConvert.SerializeObject);
    }

    [Fact]
    public async Task Get()
    {
        var query =
            """
            {
              companies
              {
                id
              }
            }
            """;
        using var response = await clientQueryExecutor.ExecuteGet(client, query);
        response.EnsureSuccessStatusCode();
        await Verify(await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Post()
    {
        var query =
            """
            {
              companies
              {
                id
              }
            }
            """;
        using var response = await clientQueryExecutor.ExecutePost(client, query);
        response.EnsureSuccessStatusCode();
        await Verify(await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Single()
    {
        var query =
            """
            query ($id: ID!)
            {
              company(id:$id)
              {
                id
              }
            }
            """;
        var variables = new
        {
            id = "1"
        };

        using var response = await clientQueryExecutor.ExecuteGet(client, query, variables);
        response.EnsureSuccessStatusCode();
        await Verify(await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task First()
    {
        var query =
            """
            query ($id: ID!)
            {
              companyFirst(id:$id)
              {
                id
              }
            }
            """;
        var variables = new
        {
            id = "1"
        };

        using var response = await clientQueryExecutor.ExecuteGet(client, query, variables);
        response.EnsureSuccessStatusCode();
        await Verify(await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Single_not_found()
    {
        var query =
            """
            query ($id: ID!)
            {
              company(id:$id)
              {
                id
              }
            }
            """;
        var variables = new
        {
            id = "99"
        };

        await ThrowsTask(() => clientQueryExecutor.ExecuteGet(client, query, variables))
            .IgnoreStackTrace();
    }

    [Fact]
    public async Task First_not_found()
    {
        var query =
            """
            query ($id: ID!)
            {
              companyFirst(id:$id)
              {
                id
              }
            }
            """;
        var variables = new
        {
            id = "99"
        };

        var response = await clientQueryExecutor.ExecuteGet(client, query, variables);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Variable()
    {
        var query =
            """
            query ($id: ID!)
            {
              companies(ids:[$id])
              {
                id
              }
            }
            """;
        var variables = new
        {
            id = "1"
        };

        using var response = await clientQueryExecutor.ExecuteGet(client, query, variables);
        response.EnsureSuccessStatusCode();
        await Verify(await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Companies_paging()
    {
        var after = 1;
        var query = $$"""
            query {
              companiesConnection(first:2, after:"{{after}}") {
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
            }
            """;
        using var response = await clientQueryExecutor.ExecuteGet(client, query);
        response.EnsureSuccessStatusCode();
        await Verify(await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Employee_summary()
    {
        var query =
            """
            query {
              employeeSummary {
                companyId
                averageAge
              }
            }
            """;
        using var response = await clientQueryExecutor.ExecuteGet(client, query);
        response.EnsureSuccessStatusCode();
        await Verify(await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Complex_query_result()
    {
        var query =
            """
            query {
              employees (
                where: [
                  {groupedExpressions: [
                    {path: "content", comparison: contains, value: "4", connector: or},

                      { path: "content", comparison: contains, value: "2"}
                  ], connector: and},
                  {path: "age", comparison: greaterThanOrEqual, value: "31"}
                ]
              ) {
                id
              }
            }
            """;
        using var response = await clientQueryExecutor.ExecuteGet(client, query);
        var result = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();
        await Verify(result);
    }

    static TestServer GetTestServer()
    {
        var builder = new WebHostBuilder();
        builder.UseStartup<Startup>();
        return new(builder);
    }
}

#endregion