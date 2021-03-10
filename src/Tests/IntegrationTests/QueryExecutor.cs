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
        Filters? filters,
        bool disableTracking)
        where TDbContext : DbContext
    {
        query = query.Replace("'", "\"");
        EfGraphQLConventions.RegisterInContainer(
            services,
            _ => data,
            data.Model,
            _ => filters,
            disableTracking);
        EfGraphQLConventions.RegisterConnectionTypesInContainer(services);
        await using var provider = services.BuildServiceProvider();
        using Schema schema = new(provider);
        EfDocumentExecuter documentExecuter = new();

        ExecutionOptions executionOptions = new()
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