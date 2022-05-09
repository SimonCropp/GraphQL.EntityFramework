using EfLocalDb;
using GraphQL;
using GraphQL.EntityFramework;
using GraphQL.Utilities;
using Microsoft.Extensions.DependencyInjection;

[UsesVerify]
public class MappingTests
{
    static SqlInstance<MappingContext> sqlInstance;

    static MappingTests() =>
        sqlInstance = new(builder => new(builder.Options));

    [Fact]
    public async Task SchemaPrint()
    {
        var services = new ServiceCollection();
        EfGraphQLConventions.RegisterInContainer<MappingContext>(services, model:sqlInstance.Model);
        services.AddSingleton<MappingChildGraphType>();
        services.AddSingleton<MappingParentGraphType>();
        services.AddSingleton<MappingSchema>();
        await using var provider = services.BuildServiceProvider();
        var mappingSchema = provider.GetRequiredService<MappingSchema>();

        var printer = new SchemaPrinter(mappingSchema);
        var print = printer.Print();
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
        EfGraphQLConventions.RegisterInContainer(services,_ => database.NewDbContext(), model:sqlInstance.Model);
        await using var provider = services.BuildServiceProvider();
        var mappingQuery = provider.GetRequiredService<MappingQuery>();

        var resolve = await mappingQuery.Fields
            .Single(x => x.Name == "children")
            .Resolver!
            .ResolveAsync(new ResolveFieldContext());
        await Verify(resolve);
    }

    [Fact]
    public async Task PropertyToObject()
    {
        var expression = Mapper<MappingContext>.PropertyToObject<MappingParent>("Property");
        var result = expression.Compile()(new() {Property = "value"});
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