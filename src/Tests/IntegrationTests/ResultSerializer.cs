using System.Threading.Tasks;
using GraphQL;
using GraphQL.NewtonsoftJson;

static class ResultSerializer
{
    static DocumentWriter writer = new(true);

    public static Task<string> Serialize(this ExecutionResult result)
    {
        return writer.WriteToStringAsync(result);
    }
}