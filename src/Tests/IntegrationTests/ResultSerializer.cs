using GraphQL;
using GraphQL.NewtonsoftJson;

static class ResultSerializer
{
    static DocumentWriter writer = new(true);

    public static Task<string> Serialize(this ExecutionResult result) =>
        writer.WriteToStringAsync(result);
}