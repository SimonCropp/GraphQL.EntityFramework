using System.Collections.Generic;
using System.Threading.Tasks;
using ApprovalTests.Namers;
using ObjectApproval;
using Xunit;

public class ConnectionConverterTests
{
    [Theory]
    [InlineData(2, null, null,null)]
    public async Task Run(int? first, int? after, int? last, int? before)
    {
        NamerFactory.AdditionalInformation = $"first_{first}_after_{after}_last_{last}_before_{before}";
        var list = new List<string>
        {
            "a", "b", "c", "d", "e", "f", "g", "h", "i", "j"
        };
        var connection = await ConnectionConverter.ApplyConnectionContext(new AsyncEnumerable<string>(list), first, after, last, before);

        ObjectApprover.VerifyWithJson(connection);
    }

}