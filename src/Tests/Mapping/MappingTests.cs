using System.Threading.Tasks;
using EfLocalDb;
using GraphQL.EntityFramework;
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
        await using var database = await sqlInstance.Build();
        var context = database.Context;

        var efGraphQlService = new EfGraphQLService<MappingContext>(context.Model, userContext => null!);
        var printer = new SchemaPrinter(new MappingSchema(efGraphQlService));
        var print = printer.Print();
        await Verify(print);
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

    public MappingTests(ITestOutputHelper output) :
        base(output)
    {
    }
}