using System.Net;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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
        return Execute(dataContext, query);
    }

    [HttpGet]
    public Task<ExecutionResult> Get(
        [BindRequired, FromQuery] GraphQlQuery query,
        [FromServices] MyDataContext dataContext)
    {
        return Execute(dataContext, query);
    }

    async Task<ExecutionResult> Execute(MyDataContext dataContext, GraphQlQuery query)
    {
        var executionOptions = new ExecutionOptions
        {
            Schema = schema,
            Query = query.Query,
            OperationName = query.OperationName,
            Inputs = query.Variables.ToInputs(),
            UserContext = dataContext,
#if (DEBUG)
            ExposeExceptions = true
#endif
        };

        var result = await executer.ExecuteAsync(executionOptions);

        if (result.Errors?.Count > 0)
        {
            Response.StatusCode = (int) HttpStatusCode.BadRequest;
        }

        return result;
    }
}