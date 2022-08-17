using GraphQL;
using GraphQL.SystemTextJson;

static class ResultSerializer
{
    static GraphQLSerializer writer = new(true);

    public static Task<string> Serialize(this ExecutionResult result)
    {
        var stream = new MemoryStream();
        writer.WriteAsync(stream,result);
        stream.Position = 0;
        var reader = new StreamReader(stream);
        return reader.ReadToEndAsync();
    }
}