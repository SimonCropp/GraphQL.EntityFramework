using System.Threading.Tasks;
using GraphQL.Types;
using GraphQL.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class SchemaPrint
{
    [Fact]
    public async Task Print()
    {
        ServiceCollection services = new();
        services.AddSingleton<ILoggerFactory>(_ => NullLoggerFactory.Instance);
        new Startup().ConfigureServices(services);

        await using var provider = services.BuildServiceProvider();
        var schema = ServiceProviderServiceExtensions.GetRequiredService<ISchema>(provider);
        var printer = new SchemaPrinter(schema);
        var print = printer.Print();
        await Verifier.Verify(print);
    }
}