namespace GraphQL.EntityFramework;

public static class GraphQlExtensions
{
    #region ExecuteWithErrorCheck

    public static async Task<ExecutionResult> ExecuteWithErrorCheck(
        this IDocumentExecuter executer,
        ExecutionOptions options)
    {
        var executionResult = await executer.ExecuteAsync(options);

        var errors = executionResult.Errors;
        if (errors is { Count: > 0 })
        {
            if (errors.Count == 1)
            {
                throw errors.First();
            }

            throw new AggregateException(errors);
        }

        return executionResult;
    }

    #endregion
}