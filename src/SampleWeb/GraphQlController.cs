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
#if (DEBUG)
            ThrowOnUnhandledException = true,
            EnableMetrics = true,
#endif
        };
        var executeAsync = await executer.ExecuteAsync(options);

        await writer.WriteAsync(Response.Body, executeAsync, cancel);
    }
}
#endregion