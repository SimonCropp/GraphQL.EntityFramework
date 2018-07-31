using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

public class GraphQlControllerTests
{
    [Fact]
    public async Task Integration_Get()
    {
        using (var server = GetTestServer())
        using (var client = server.CreateClient())
        using (var postRequest = new HttpRequestMessage(HttpMethod.Get, "graphql?query={companies{id}}"))
        {
            await SendAndVerify(client, postRequest);
        }
    }

    [Fact]
    public async Task Integration_Post()
    {
        using (var server = GetTestServer())
        using (var client = server.CreateClient())
        using (var postRequest = new HttpRequestMessage(HttpMethod.Post, "graphql")
        {
            Content = new StringContent("{\"query\":\"{companies{id}}\",\"variables\":null,\"operationName\":null}", Encoding.UTF8, "application/json")
        })
        {
            await SendAndVerify(client, postRequest);
        }
    }

    static TestServer GetTestServer()
    {
        var hostBuilder = new WebHostBuilder();
        hostBuilder.UseStartup<Startup>();
        return new TestServer(hostBuilder);
    }

    static async Task SendAndVerify(HttpClient client, HttpRequestMessage request)
    {
        using (var response = await client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            Assert.Equal("{\"data\":{\"companies\":[{\"id\":1},{\"id\":4},{\"id\":6},{\"id\":7}]}}", result);
        }
    }
}