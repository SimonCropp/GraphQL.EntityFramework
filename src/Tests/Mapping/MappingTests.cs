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
        sqlInstance = new SqlInstance<MappingContext>(
            constructInstance: builder => new MappingContext(builder.Options));
    }

    [Fact]
    public async Task Run()
    {
        GraphTypeTypeRegistry.Register<MappingEntity1, MappingEntity1Graph>();
        await using var database = await sqlInstance.Build();

        var property = typeof(MappingEntity1).GetProperty("Property")!;
        await Verify(Mapper.BuildPropertyLambda<MappingEntity1>(property).ToString());
    }

    public MappingTests(ITestOutputHelper output) :
        base(output)
    {
    }
}