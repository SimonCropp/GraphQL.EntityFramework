using ExecutionContext = GraphQL.Execution.ExecutionContext;

public class MappingTests
{
    static SqlInstance<MappingContext> sqlInstance;

    static MappingTests()
    {
        Mapper<MappingContext>.AddIgnoredName("IgnoreByName");
        sqlInstance = new(builder =>
        {
            builder.ConfigureWarnings(_ =>
                _.Ignore(
                    CoreEventId.NavigationBaseIncludeIgnored,
                    CoreEventId.ShadowForeignKeyPropertyCreated,
                    CoreEventId.RowLimitingOperationWithoutOrderByWarning,
                    CoreEventId.CollectionWithoutComparer));
            return new(builder.Options);
        });
    }

    [Fact]
    public async Task SchemaPrint()
    {
        var services = new ServiceCollection();
        EfGraphQLConventions.RegisterInContainer<MappingContext>(services, model: sqlInstance.Model);
        services.AddSingleton<MappingChildGraphType>();
        services.AddSingleton<MappingParentGraphType>();
        services.AddSingleton<MappingSchema>();
        services.AddGraphQL(null);
        await using var provider = services.BuildServiceProvider();
        var schema = provider.GetRequiredService<MappingSchema>();

        var print = schema.Print();
        await Verify(print);
    }

    [Fact]
    public async Task Resolve()
    {
        await using var database = await sqlInstance.Build();

        var parent = new MappingParent();
        var child = new MappingChild
        {
            Parent = parent
        };
        await database.AddDataUntracked(child, parent);
        var services = new ServiceCollection();
        services.AddSingleton<MappingQuery>();
        EfGraphQLConventions.RegisterInContainer(services, (_, _) => database.NewDbContext(), model: sqlInstance.Model);
        await using var provider = services.BuildServiceProvider();
        var mappingQuery = provider.GetRequiredService<MappingQuery>();

        var fieldContext = new ResolveFieldContext
        {
            ExecutionContext = new ExecutionContext
            {
                RequestServices = provider
            }
        };
        var resolve = await mappingQuery.Fields
            .Single(_ => _.Name == "children")
            .Resolver!
            .ResolveAsync(fieldContext);
        await Verify(resolve);
    }

    [Fact]
    public async Task PropertyToObject()
    {
        var expression = Mapper<MappingContext>.PropertyToObject<MappingParent>("Property");
        var result = expression.Compile()(new()
        {
            Property = "value"
        });
        await Verify(
            new
            {
                expression,
                result
            });
    }

    [Fact]
    public async Task NavigationProperty()
    {
        await using var database = await sqlInstance.Build();
        var context = database.Context;

        var child = new MappingChild();
        var parent = new MappingParent
        {
            Property = "value"
        };
        child.Parent = parent;
        await database.AddData(child, parent);
        var expression = Mapper<MappingContext>.NavigationExpression<MappingChild, MappingParent>("Parent");
        var compile = expression.Compile();
        var result = compile(
            new()
            {
                DbContext = context,
                Source = child
            });
        await Verify(
            new
            {
                expression,
                result
            });
    }
}