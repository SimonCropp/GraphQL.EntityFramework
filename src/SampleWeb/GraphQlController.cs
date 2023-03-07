using GraphQL.SystemTextJson;
using GraphQL.Types;
using Microsoft.AspNetCore.Mvc;

#region GraphQlController
[Route("[controller]")]
[ApiController]
public class GraphQlController :
    Controller
{
    IDocumentExecuter executer;
    ISchema schema;
    static GraphQLSerializer writer = new(true);

    public GraphQlController(ISchema schema, IDocumentExecuter executer)
    {
        this.schema = schema;
        this.executer = executer;
    }

    [HttpGet]
    public Task Get(
        [FromQuery] string query,
        [FromQuery] string? variables,
        [FromQuery] string? operationName,
        Cancellation cancellation)
    {
        var inputs = variables.ToInputs();
        return Execute(query, operationName, inputs, cancellation);
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
        Cancellation cancellation)
    {
        var inputs = query.Variables.ToInputs();
        return Execute(query.Query, query.OperationName, inputs, cancellation);
    }

    async Task Execute(string query,
        string? operationName,
        Inputs? variables,
        Cancellation cancellation)
    {
        var options = new ExecutionOptions
        {
            Schema = schema,
            Query = query,
            OperationName = operationName,
            Variables = variables,
            CancellationToken = cancellation,
#if (DEBUG)
            ThrowOnUnhandledException = true,
            EnableMetrics = true,
#endif
        };
        var executeAsync = await executer.ExecuteAsync(options);

        await writer.WriteAsync(Response.Body, executeAsync, cancellation);
    }
}
#endregion