using System;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.EntityFramework
{
    public static class GraphQlExtensions
    {
        #region ExecuteWithErrorCheck

        public static async Task<ExecutionResult> ExecuteWithErrorCheck(this DocumentExecuter documentExecuter, ExecutionOptions executionOptions)
        {
            Guard.AgainstNull(nameof(documentExecuter),documentExecuter);
            Guard.AgainstNull(nameof(executionOptions),executionOptions);
            var executionResult = await documentExecuter.ExecuteAsync(executionOptions)
                .ConfigureAwait(false);

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