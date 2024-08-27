using GraphQL;

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
        var print = schema.Print();
        await Verify(print);
    }
}