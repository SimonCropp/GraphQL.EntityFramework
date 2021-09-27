﻿using System.Net.Http;
using VerifyXunit;
using GraphQL.EntityFramework.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;
// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

#region GraphQlControllerTests

[UsesVerify]
public class GraphQlControllerTests
{
    static HttpClient client = null!;
    static ClientQueryExecutor clientQueryExecutor;
    static WebSocketClient webSocket = null!;

    static GraphQlControllerTests()
    {
        var server = GetTestServer();
        client = server.CreateClient();
        webSocket = server.CreateWebSocketClient();
        webSocket.ConfigureRequest =
            request =>
            {
                var headers = request.Headers;
                headers["Sec-WebSocket-Protocol"] = "graphql-ws";
            };
        clientQueryExecutor = new(JsonConvert.SerializeObject);
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
        using var response = await clientQueryExecutor.ExecuteGet(client, query);
        response.EnsureSuccessStatusCode();
        await Verifier.Verify(await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Get_single()
    {
        var query = @"
query ($id: ID!)
{
  company(id:$id)
  {
    id
  }
}";
        var variables = new
        {
            id = "1"
        };

        using var response = await clientQueryExecutor.ExecuteGet(client, query, variables);
        response.EnsureSuccessStatusCode();
        await Verifier.Verify(await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Get_single_not_found()
    {
        var query = @"
query ($id: ID!)
{
  company(id:$id)
  {
    id
  }
}";
        var variables = new
        {
            id = "99"
        };

        using var response = await clientQueryExecutor.ExecuteGet(client, query, variables);
        var result = await response.Content.ReadAsStringAsync();
        Assert.Contains("Not found", result);
    }

    [Fact]
    public async Task Get_variable()
    {
        var query = @"
query ($id: ID!)
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

        using var response = await clientQueryExecutor.ExecuteGet(client, query, variables);
        response.EnsureSuccessStatusCode();
        await Verifier.Verify(await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Get_companies_paging()
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
        using var response = await clientQueryExecutor.ExecuteGet(client, query);
        response.EnsureSuccessStatusCode();
        await Verifier.Verify(await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Get_employee_summary()
    {
        var query = @"
query {
  employeeSummary {
    companyId
    averageAge
  }
}";
        using var response = await clientQueryExecutor.ExecuteGet(client, query);
        response.EnsureSuccessStatusCode();
        await Verifier.Verify(await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Get_complex_query_result()
    {
        var query = @"
query {
  employees (
    where: [
      {groupedExpressions: [
        {path: ""content"", comparison: contains, value: ""4"", connector: or},

          { path: ""content"", comparison: contains, value: ""2""}
      ], connector: and},
      {path: ""age"", comparison: greaterThanOrEqual, value: ""31""}
  	]
  ) {
    id
  }
}";
        using var response = await clientQueryExecutor.ExecuteGet(client, query);
        var result = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();
        await Verifier.Verify(result);
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
        using var response = await clientQueryExecutor.ExecutePost(client, query);
        var result = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();
        await Verifier.Verify(result);
    }

    [Fact]
    public async Task Post_variable()
    {
        var query = @"
query ($id: ID!)
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
        using var response = await clientQueryExecutor.ExecutePost(client, query, variables);
        var result = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();
        await Verifier.Verify(result);
    }

    //TODO: https://github.com/graphql-dotnet/graphql-client
  //  [Fact]
//    public async Task Should_subscribe_to_companies()
//    {
//        var resetEvent = new AutoResetEvent(false);

//        var result = new GraphQLHttpSubscriptionResult(
//            new Uri("http://example.com/graphql"),
//            new GraphQLRequest
//            {
//                Query = @"
//subscription
//{
//  companyChanged
//  {
//    id
//  }
//}"
//            },
//            websocketClient,
//            response =>
//            {
//                if (response is null)
//                {
//                    return;
//                }

//                Assert.Null(response.Errors);

//                if (response.Data is not null)
//                {
//                    resetEvent.Set();
//                }
//            });


//        var cancellationSource = new CancellationTokenSource();

//        var task = result.StartAsync(cancellationSource.Token);

//        Assert.True(resetEvent.WaitOne(TimeSpan.FromSeconds(10)));

//        cancellationSource.Cancel();

//        await task;
//    }

    static TestServer GetTestServer()
    {
        WebHostBuilder hostBuilder = new();
        hostBuilder.UseStartup<Startup>();
        return new(hostBuilder);
    }
}

#endregion