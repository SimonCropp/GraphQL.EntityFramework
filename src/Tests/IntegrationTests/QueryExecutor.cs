using System;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

static class QueryExecutor
{
    public static async Task<object> ExecuteQuery(string queryString, ServiceCollection services, DbContext dataContext, Inputs inputs)
    {
        queryString = queryString.Replace("'", "\"");
        EfGraphQLConventions.RegisterConnectionTypesInContainer(services);
        EfGraphQLConventions.RegisterInContainer(services, dataContext);
        using (var provider = services.BuildServiceProvider())
        using (var schema = new Schema(new FuncDependencyResolver(provider.GetRequiredService)))
        {
            var documentExecuter = new DocumentExecuter();

            var executionOptions = new ExecutionOptions
            {
                Schema = schema,
                Query = queryString,
                UserContext = dataContext,
                Inputs = inputs
            };

            var executionResult = await documentExecuter.ExecuteAsync(executionOptions).ConfigureAwait(false);

            if (executionResult.Errors != null && executionResult.Errors.Count > 0)
            {
                if (executionResult.Errors.Count == 1)
                {
                    throw executionResult.Errors.First();
                }

                throw new AggregateException(executionResult.Errors);
            }

            return executionResult.Data;
        }
    }
}