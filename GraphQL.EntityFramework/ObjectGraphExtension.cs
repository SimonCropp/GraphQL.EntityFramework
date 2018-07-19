using System;
using System.Threading.Tasks;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public static partial class ObjectGraphExtension
    {
        static Type MakeListGraphType(Type graphType)
        {
            var listGraphType = typeof(ListGraphType<>);
            return listGraphType.MakeGenericType(graphType);
        }

        static TResult ExecuteQuery<TResult>(string fieldName, Type graph, ExecutionErrors errors, Func<TResult> func)
        {
            try
            {
                return func();
            }
            catch (ErrorException exception)
            {
                AddError(fieldName, graph, errors, exception.Message);
                throw;
            }
            catch (Exception)
            {
                AddError(fieldName, graph, errors);
                throw;
            }
        }

        static async Task<TResult> ExecuteAsyncQuery<TResult>(string fieldName, Type graph, ExecutionErrors errors, Func<Task<TResult>> func)
        {
            try
            {
                return await func().ConfigureAwait(false);
            }
            catch (ErrorException exception)
            {
                AddError(fieldName, graph, errors, exception.Message);
                throw;
            }
            catch (Exception)
            {
                AddError(fieldName, graph, errors);
                throw;
            }
        }

        static async Task<object> ExecuteConnection<TResult>(string fieldName, Type graph, ExecutionErrors errors, Func<Task<TResult>> func)
        {
            try
            {
                return await func().ConfigureAwait(false);
            }
            catch (ErrorException exception)
            {
                AddError(fieldName, graph, errors, exception.Message);
                throw;
            }
            catch (Exception)
            {
                AddError(fieldName, graph, errors);
                throw;
            }
        }

        static void AddError(string fieldName, Type graph, ExecutionErrors errors, string message=null)
        {
            var error = $"Failed to execute query for field '{fieldName}' on graph '{graph.FullName}'.";
            if (message != null)
            {
                error = error + $" {message}";
            }
            errors.Add(new ExecutionError(error));
        }
    }
}