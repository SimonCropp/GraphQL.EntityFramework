using System.Collections.Generic;
using System.Threading.Tasks;
using ApprovalTests.Namers;
using ObjectApproval;
using Xunit;

public class ConnectionConverterTests
{
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

    ////first after
    [InlineData(2, null, null, 2)]
    [InlineData(3, null, null, 2)]
    [InlineData(2, null, null, 3)]

    ////last before
    [InlineData(null, null, 2, null)]
    [InlineData(null, null, 2, 8)]

    //last after
    [InlineData(null, 7, 2, null)]

    public async Task Run(int? first, int? after, int? last, int? before)
    {
        NamerFactory.AdditionalInformation = $"first_{first}_after_{after}_last_{last}_before_{before}";
        var list = new List<string>
        {
            "a", "b", "c", "d", "e", "f", "g", "h", "i", "j"
        };
        var queryable = new AsyncEnumerable<string>(list);
        var connection = await ConnectionConverter.ApplyConnectionContext(queryable, first, after, last, before);

        ObjectApprover.VerifyWithJson(connection);
    }
}