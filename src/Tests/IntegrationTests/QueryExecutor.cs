static class QueryExecutor
{
    public static async Task<string> ExecuteQuery<TDbContext>(
        string query,
        ServiceCollection services,
        TDbContext data,
        Inputs? inputs,
        Filters<TDbContext>? filters,
        bool disableTracking)
        where TDbContext : DbContext
    {
        EfGraphQLConventions.RegisterInContainer(
            services,
            (_, _) => data,
            data.Model,
            _ => filters,
            disableTracking);
        await using var provider = services.BuildServiceProvider();
        using var schema = new Schema(provider);
        var executer = new EfDocumentExecuter();

        var options = new ExecutionOptions
        {
            Schema = schema,
            Query = query,
            ThrowOnUnhandledException = true,
            Variables = inputs,
            RequestServices = provider,
        };

        var result = await executer.ExecuteWithErrorCheck(options);
        return await result.Serialize();
    }
}