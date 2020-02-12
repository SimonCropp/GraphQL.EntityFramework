using System;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.EntityFramework
{
    public static class GraphQlExtensions
    {
        #region ExecuteWithErrorCheck

        public static async Task<ExecutionResult> ExecuteWithErrorCheck(
            this IDocumentExecuter executer,
            ExecutionOptions options)
        {
            Guard.AgainstNull(nameof(executer), executer);
            Guard.AgainstNull(nameof(options), options);
            var executionResult = await executer.ExecuteAsync(options);

            var errors = executionResult.Errors;
            if (errors != null && errors.Count > 0)
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
}