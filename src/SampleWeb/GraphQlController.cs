using GraphQL;
using GraphQL.NewtonsoftJson;
using GraphQL.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;

#region GraphQlController
[Route("[controller]")]
[ApiController]
public class GraphQlController :
    Controller
{
    IDocumentExecuter executer;
    ISchema schema;
    static DocumentWriter writer = new(true);

    public GraphQlController(ISchema schema, IDocumentExecuter executer)
    {
        this.schema = schema;
        this.executer = executer;
    }

    [HttpPost]
    public Task Post(
        [BindRequired, FromBody] PostBody body,
        CancellationToken cancellation)
    {
        return Execute(body.Query, body.OperationName, body.Variables, cancellation);
    }

    public class PostBody
    {
        public string? OperationName;
        public string Query = null!;
        public JObject? Variables;
    }

    [HttpGet]
    public Task Get(
        [FromQuery] string query,
        [FromQuery] string? variables,
        [FromQuery] string? operationName,
        CancellationToken cancellation)
    {
        var jObject = ParseVariables(variables);
        return Execute(query, operationName, jObject, cancellation);
    }

    async Task Execute(string query,
        string? operationName,
        JObject? variables,
        CancellationToken cancellation)
    {
        var options = new ExecutionOptions
        {
            Schema = schema,
            Query = query,
            OperationName = operationName,
            Inputs = variables?.ToInputs(),
            CancellationToken = cancellation,
#if (DEBUG)
            ThrowOnUnhandledException = true,
            EnableMetrics = true,
#endif
        };
        var executeAsync = await executer.ExecuteAsync(options);

        await writer.WriteAsync(Response.Body, executeAsync, cancellation);
    }

    static JObject? ParseVariables(string? variables)
    {
        if (variables is null)
        {
            return null;
        }

        try
        {
            return JObject.Parse(variables);
        }
        catch (Exception exception)
        {
            throw new("Could not parse variables.", exception);
        }
    }
}
#endregion