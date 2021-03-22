using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EfLocalDb;
using GraphQL;
using GraphQL.EntityFramework;
using GraphQL.Utilities;
using Microsoft.Extensions.DependencyInjection;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class MappingTests
{
    static SqlInstance<MappingContext> sqlInstance;

    static MappingTests()
    {
        sqlInstance = new(builder => new(builder.Options));
    }

    [Fact]
    public async Task SchemaPrint()
    {
        ServiceCollection services = new();
        EfGraphQLConventions.RegisterInContainer<MappingContext>(services);
        services.AddSingleton<MappingChildGraph>();
        services.AddSingleton<MappingParentGraph>();
        await using var provider = services.BuildServiceProvider();
        var mappingSchema = provider.GetRequiredService<MappingSchema>();

        SchemaPrinter printer = new(mappingSchema);
        var print = printer.Print();
        await Verifier.Verify(print);
    }

    [Fact]
    public async Task Resolve()
    {
        await using var database = await sqlInstance.Build();

        MappingParent parent = new();
        MappingChild child = new()
        {
            Parent = parent
        };
        await database.AddDataUntracked(child, parent);
        ServiceCollection services = new();
        await using var provider = services.BuildServiceProvider();
        EfGraphQLConventions.RegisterInContainer<MappingContext>(services);
        var mappingQuery = provider.GetRequiredService<MappingQuery>();

        var resolve = await (Task<IEnumerable<MappingChild>>)mappingQuery.Fields
            .Single(x => x.Name == "children")
            .Resolver
            .Resolve(new ResolveFieldContext());
        await Verifier.Verify(resolve);
    }

    [Fact]
    public async Task PropertyToObject()
    {
        var expression = Mapper<MappingContext>.PropertyToObject<MappingParent>("Property");
        var result = expression.Compile()(new MappingParent {Property = "value"});
        await Verifier.Verify(
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

        MappingChild child = new();
        MappingParent parent = new()
        {
            Property = "value"
        };
        child.Parent = parent;
        await database.AddData(child, parent);
        var expression = Mapper<MappingContext>.NavigationExpression<MappingChild, MappingParent>("Parent");
        var compile = expression.Compile();
        var result = compile(
            new ResolveEfFieldContext<MappingContext, MappingChild>
            {
                DbContext = context,
                Source = child
            });
        await Verifier.Verify(
            new
            {
                expression,
                result
            });
    }
}