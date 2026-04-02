using GraphQL;

public class SchemaPrint
{
    [Fact]
    public async Task Print()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(_ => NullLoggerFactory.Instance);

        EfGraphQLConventions.RegisterInContainer<SampleDbContext>(
            services,
            model: SampleDbContext.StaticModel);

        foreach (var type in typeof(Program).Assembly
                     .GetTypes()
                     .Where(_ => !_.IsAbstract &&
                                 (_.IsAssignableTo(typeof(IObjectGraphType)) ||
                                  _.IsAssignableTo(typeof(IInputObjectGraphType)))))
        {
            services.AddSingleton(type);
        }

        services.AddSingleton<IDocumentExecuter, EfDocumentExecuter>();
        services.AddSingleton<ISchema, Schema>();
        services.AddGraphQL(null);

        await using var provider = services.BuildServiceProvider();
        var schema = provider.GetRequiredService<ISchema>();
        var print = schema.Print();
        await Verify(print);
    }
}
