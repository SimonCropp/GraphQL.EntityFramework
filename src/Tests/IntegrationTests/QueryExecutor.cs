using System.Threading.Tasks;
using GraphQL;
using GraphQL.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

static class QueryExecutor
{
    public static async Task<object> ExecuteQuery<TDbContext>(
        string query,
        ServiceCollection services,
        TDbContext data,
        Inputs? inputs,
        Filters? filters)
        where TDbContext : DbContext
    {
        query = query.Replace("'", "\"");
        EfGraphQLConventions.RegisterInContainer(
            services,
            userContext => data,
            data.Model,
            userContext => filters);
        EfGraphQLConventions.RegisterConnectionTypesInContainer(services);
        await using var provider = services.BuildServiceProvider();
        using var schema = new Schema(provider);
        var documentExecuter = new EfDocumentExecuter();

        var executionOptions = new ExecutionOptions
        {
            Schema = schema,
            Query = query,
            UserContext = new UserContextSingleDb<TDbContext>(data),
            Inputs = inputs,
        };

        var executionResult = await documentExecuter.ExecuteWithErrorCheck(executionOptions);
        return executionResult.Data;
    }
}