using GraphQL.Types;
using GraphQL.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

[UsesVerify]
public class SchemaPrint
{
    [Fact]
    public async Task Print()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(_ => NullLoggerFactory.Instance);
        new Startup().ConfigureServices(services);

        await using var provider = services.BuildServiceProvider();
        var schema = provider.GetRequiredService<ISchema>();
        var printer = new SchemaPrinter(schema);
        var print = printer.Print();
        await Verify(print);
    }
}