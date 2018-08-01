using System.Net;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;

[Route("[controller]")]
[ApiController]
public class GraphQlController : Controller
{
    IDocumentExecuter executer;
    ISchema schema;

    public GraphQlController(ISchema schema, IDocumentExecuter executer)
    {
        this.schema = schema;
        this.executer = executer;
    }

    [HttpPost]
    public Task<ExecutionResult> Post(
        [BindRequired, FromBody] GraphQlQuery query,
        [FromServices] MyDataContext dataContext)
    {
        return Execute(dataContext, query.Query, query.OperationName, query.Variables);
    }

    [HttpGet]
    public Task<ExecutionResult> Get(
        [BindRequired, FromQuery] string query,
        [FromQuery] string operationName,
        [FromServices] MyDataContext dataContext)
    {
        return Execute(dataContext, query, operationName, null);
    }

    async Task<ExecutionResult> Execute(MyDataContext dataContext, string query, string operationName, JObject variables)
    {
        var executionOptions = new ExecutionOptions
        {
            Schema = schema,
            Query = query,
            OperationName = operationName,
            Inputs = variables.ToInputs(),
            UserContext = dataContext
        };

        var result = await executer.ExecuteAsync(executionOptions);

        if (result.Errors?.Count > 0)
        {
            Response.StatusCode = (int) HttpStatusCode.BadRequest;
        }

        return result;
    }
}