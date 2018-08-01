using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

public class GraphQlControllerTests
{
    [Fact]
    public async Task Get()
    {
        using (var server = GetTestServer())
        using (var client = server.CreateClient())
        using (var postRequest = new HttpRequestMessage(HttpMethod.Get, "graphql?query={companies{id}}"))
        using (var response = await client.SendAsync(postRequest))
        {
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            Assert.Equal("{\"data\":{\"companies\":[{\"id\":1},{\"id\":4},{\"id\":6},{\"id\":7}]}}", result);
        }
    }

    [Fact]
    public async Task Get_null_query()
    {
        using (var server = GetTestServer())
        using (var client = server.CreateClient())
        using (var postRequest = new HttpRequestMessage(HttpMethod.Get, "graphql"))
        using (var response = await client.SendAsync(postRequest))
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            Assert.Equal("{\"Query\":[\"The Query field is required.\"]}", result);
        }
    }

    [Fact]
    public async Task Post()
    {
        using (var server = GetTestServer())
        using (var client = server.CreateClient())
        using (var postRequest = new HttpRequestMessage(HttpMethod.Post, "graphql")
        {
            Content = new StringContent("{\"query\":\"{companies{id}}\",\"variables\":null}", Encoding.UTF8, "application/json")
        })
        using (var response = await client.SendAsync(postRequest))
        {
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            Assert.Equal("{\"data\":{\"companies\":[{\"id\":1},{\"id\":4},{\"id\":6},{\"id\":7}]}}", result);
        }
    }

    [Fact]
    public async Task Post_null_qery()
    {
        using (var server = GetTestServer())
        using (var client = server.CreateClient())
        using (var postRequest = new HttpRequestMessage(HttpMethod.Post, "graphql")
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        })
        using (var response = await client.SendAsync(postRequest))
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            Assert.Equal("{\"Query\":[\"The Query field is required.\"]}", result);
        }
    }

    static TestServer GetTestServer()
    {
        var hostBuilder = new WebHostBuilder();
        hostBuilder.UseStartup<Startup>();
        return new TestServer(hostBuilder);
    }
}