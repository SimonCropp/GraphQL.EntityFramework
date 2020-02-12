using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

public class Program
{
    public static Task Main()
    {
        var webHostBuilder = WebHost.CreateDefaultBuilder();
        var hostBuilder = webHostBuilder.UseStartup<Startup>();
        return hostBuilder.Build().RunAsync();
    }
}