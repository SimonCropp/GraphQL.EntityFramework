using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

public class Program
{
    public static async Task Main()
    {
        await DbContextBuilder.Start();
        var webHostBuilder = WebHost.CreateDefaultBuilder();
        var hostBuilder = webHostBuilder.UseStartup<Startup>();
        hostBuilder.Build().Run();
    }
}