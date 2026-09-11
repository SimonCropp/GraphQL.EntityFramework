static class QueryExecutor
{
    public static async Task<ExecutionResult> ExecuteQuery<TDbContext>(
        string query,
        ServiceCollection services,
        TDbContext data,
        Inputs? inputs,
        Filters<TDbContext>? filters,
        bool disableTracking,
        bool includeSqlInExceptions = false,
        OrderByStyle orderByStyle = OrderByStyle.Enum)
        where TDbContext : DbContext
    {
        EfGraphQLConventions.RegisterInContainer(
            services,
            (_, _) => data,
            data.Model,
            _ => filters,
            disableTracking,
            includeSqlInExceptions,
            orderByStyle);
        await using var provider = services.BuildServiceProvider();
        using var schema = new Schema(provider);
        var executer = new EfDocumentExecuter();

        var options = new ExecutionOptions
        {
            Schema = schema,
            Query = query,
            Variables = inputs,
            RequestServices = provider,
        };

        return await executer.ExecuteWithErrorCheck(options);
    }
}
