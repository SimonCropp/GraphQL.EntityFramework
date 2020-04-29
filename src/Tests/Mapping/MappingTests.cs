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
        GraphTypeTypeRegistry.Register<MappingParent, MappingParentGraph>();
        GraphTypeTypeRegistry.Register<MappingChild, MappingChildGraph>();

        sqlInstance = new SqlInstance<MappingContext>(
            constructInstance: builder => new MappingContext(builder.Options));
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