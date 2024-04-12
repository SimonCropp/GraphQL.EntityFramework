using GraphQL.SystemTextJson;
using GraphQL.Types;
using Microsoft.AspNetCore.Mvc;

#region GraphQlController
[Route("[controller]")]
[ApiController]
public class GraphQlController(ISchema schema, IDocumentExecuter executer) :
    Controller
{
    static GraphQLSerializer writer = new(true);

    [HttpGet]
    public Task Get(
        [FromQuery] string query,
        [FromQuery] string? variables,
        [FromQuery] string? operationName,
        Cancel cancel)
    {
        var inputs = variables.ToInputs();
        return Execute(query, operationName, inputs, cancel);
    }

    public class GraphQLQuery
    {
        public string? OperationName { get; set; }
        public string Query { get; set; } = null!;
        public string? Variables { get; set; }
    }

    [HttpPost]
    public Task Post(
        [FromBody]GraphQLQuery query,
        Cancel cancel)
    {
        var inputs = query.Variables.ToInputs();
        return Execute(query.Query, query.OperationName, inputs, cancel);
    }

    async Task Execute(string query,
        string? operationName,
        Inputs? variables,
        Cancel cancel)
    {
        var options = new ExecutionOptions
        {
            Schema = schema,
            Query = query,
            OperationName = operationName,
            Variables = variables,
            CancellationToken = cancel,
            ThrowOnUnhandledException = true,
            EnableMetrics = true,
        };
        var executeAsync = await executer.ExecuteAsync(options);

        await writer.WriteAsync(Response.Body, executeAsync, cancel);
    }
}
#endregion