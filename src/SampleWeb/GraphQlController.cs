using GraphQL.SystemTextJson;
using GraphQL.Types;
using Microsoft.AspNetCore.Mvc;

#region GraphQlController
[Route("[controller]")]
[ApiController]
public class GraphQlController(ISchema schema, IDocumentExecuter executer) :
    ControllerBase
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

        // An object, as GraphQL over HTTP defines it and as every client sends it. Binding this as
        // a string rejects the whole body, so a parameterized query fails before it is parsed.
        public JsonElement? Variables { get; set; }
    }

    [HttpPost]
    public Task Post(
        [FromBody]GraphQLQuery query,
        Cancel cancel)
    {
        var inputs = query.Variables?.GetRawText().ToInputs();
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