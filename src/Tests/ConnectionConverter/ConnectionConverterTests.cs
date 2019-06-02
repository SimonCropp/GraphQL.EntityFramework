using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApprovalTests.Namers;
using GraphQL.EntityFramework;
using GraphQL.Types;
using ObjectApproval;
using Xunit;
using Xunit.Abstractions;

public class ConnectionConverterTests :
    XunitLoggingBase
{
    List<string> list = new List<string>
    {
        "a", "b", "c", "d", "e", "f", "g", "h", "i", "j"
    };

    [Theory]
    //first after
    [InlineData(1, 0, null, null)]
    [InlineData(2, null, null, null)]
    [InlineData(2, 1, null, null)]
    [InlineData(3, null, null, null)]
    [InlineData(3, 1, null, null)]
    [InlineData(10, null, null, null)]
    [InlineData(10, 1, null, null)]
    [InlineData(11, null, null, null)]
    [InlineData(11, 1, null, null)]

    //first after
    [InlineData(2, null, null, 2)]
    [InlineData(3, null, null, 2)]
    [InlineData(2, null, null, 3)]

    //last before
    [InlineData(null, null, 2, null)]
    [InlineData(null, null, 2, 8)]

    //last after
    [InlineData(null, 7, 2, null)]
    public async Task Queryable(int? first, int? after, int? last, int? before)
    {
        NamerFactory.AdditionalInformation = $"first_{first}_after_{after}_last_{last}_before_{before}";
        var queryable = new AsyncEnumerable<string>(list);
        var connection = await ConnectionConverter.ApplyConnectionContext(queryable, first, after, last, before, new ResolveFieldContext<string>(), new GlobalFilters(), CancellationToken.None);
        ObjectApprover.VerifyWithJson(connection);
    }

    [Theory]
    //first after
    [InlineData(1, 0, null, null)]
    [InlineData(2, null, null, null)]
    [InlineData(2, 1, null, null)]
    [InlineData(3, null, null, null)]
    [InlineData(3, 1, null, null)]
    [InlineData(10, null, null, null)]
    [InlineData(10, 1, null, null)]
    [InlineData(11, null, null, null)]
    [InlineData(11, 1, null, null)]

    //first after
    [InlineData(2, null, null, 2)]
    [InlineData(3, null, null, 2)]
    [InlineData(2, null, null, 3)]

    //last before
    [InlineData(null, null, 2, null)]
    [InlineData(null, null, 2, 8)]

    //last after
    [InlineData(null, 7, 2, null)]

    public void List(int? first, int? after, int? last, int? before)
    {
        NamerFactory.AdditionalInformation = $"first_{first}_after_{after}_last_{last}_before_{before}";
        var connection = ConnectionConverter.ApplyConnectionContext(list, first, after, last, before);
        ObjectApprover.VerifyWithJson(connection);
    }

    public ConnectionConverterTests(ITestOutputHelper output) :
        base(output)
    {
    }
}