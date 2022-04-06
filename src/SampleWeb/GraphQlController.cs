using System.Text.Json;
using GraphQL;
using GraphQL.SystemTextJson;
using GraphQL.Transport;
using GraphQL.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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

    [HttpPost]
    public Task Post(
        [BindRequired, FromBody] GraphQLRequest request,
        CancellationToken cancellation) =>
        Execute(request.Query, request.OperationName, request.Variables, cancellation);

    [HttpGet]
    public Task Get(
        [FromQuery] string query,
        [FromQuery] string? variables,
        [FromQuery] string? operationName,
        CancellationToken cancellation)
    {
        Inputs? inputs = null;
        if (variables != null)
        {
            inputs = JsonSerializer.Deserialize<Inputs?>(variables);
        }

        return Execute(query, operationName, inputs, cancellation);
    }

    async Task Execute(string query,
        string? operationName,
        Inputs? variables,
        CancellationToken cancellation)
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