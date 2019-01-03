using GraphQL.EntityFramework;
using Xunit.Abstractions;

public class TestBase
{
    static TestBase()
    {
        Scalars.Initialize();
    }

    public TestBase(ITestOutputHelper output)
    {
        Output = output;
    }

    protected readonly ITestOutputHelper Output;
}