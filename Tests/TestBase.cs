using EfCoreGraphQL;
using Xunit.Abstractions;

public class TestBase
{
    static TestBase()
    {
        Scalar.Inject();
    }

    public TestBase(ITestOutputHelper output)
    {
        Output = output;
    }

    protected readonly ITestOutputHelper Output;
}