using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EfLocalDb;
using GraphQL.EntityFramework;
using GraphQL.Types;
using GraphQL.Utilities;
using VerifyXunit;
using Xunit;
using Xunit.Abstractions;

public class MappingTests :
    VerifyBase
{
    static SqlInstance<MappingContext> sqlInstance;

    static MappingTests()
    {
        ArgumentGraphs.Initialise();
        GraphTypeTypeRegistry.Register<MappingParent, MappingParentGraph>();
        GraphTypeTypeRegistry.Register<MappingChild, MappingChildGraph>();

        sqlInstance = new SqlInstance<MappingContext>(
            constructInstance: builder => new MappingContext(builder.Options));
    }

    [Fact]
    public async Task SchemaPrint()
    {
        var graphQlService = new EfGraphQLService<MappingContext>(sqlInstance.Model, userContext => null!);
        var printer = new SchemaPrinter(new MappingSchema(graphQlService));
        var print = printer.Print();
        await Verify(print);
    }

    [Fact]
    public async Task Resolve()
    {
        await using var database = await sqlInstance.Build();
        var context = database.Context;

        var parent = new MappingParent();
        var child = new MappingChild
        {
            Parent = parent
        };
        await database.AddDataUntracked(child, parent);
        var graphQlService = new EfGraphQLService<MappingContext>(context.Model, _ => context);
        var resolve = await (Task<IEnumerable<MappingChild>>) new MappingQuery(graphQlService).Fields
            .Single(x => x.Name == "children")
            .Resolver
            .Resolve(new ResolveFieldContext());
        await Verify(resolve);
    }

    [Fact]
    public async Task PropertyToObject()
    {
        var property = typeof(MappingParent).GetProperty("Property")!;
        var expression = Mapper.PropertyToObject<MappingParent>(property);
        var result = expression.Compile()(new MappingParent {Property = "value"});
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
        var parent = new MappingParent {Property = "value"};
        child.Parent = parent;
        await database.AddData(child, parent);
        var expression = Mapper.NavigationExpression<MappingContext, MappingChild>("Parent");
        Func<ResolveEfFieldContext<MappingContext, MappingChild>, object?> compile = expression.Compile();
        var result = compile(
            new ResolveEfFieldContext<MappingContext, MappingChild>
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

    public MappingTests(ITestOutputHelper output) :
        base(output)
    {
    }
}