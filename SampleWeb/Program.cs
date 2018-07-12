using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

public class Program
{
    public static void Main()
    {
        var webHostBuilder = WebHost.CreateDefaultBuilder();
        var hostBuilder = webHostBuilder.UseStartup<Startup>();
        hostBuilder.Build().Run();
    }
}