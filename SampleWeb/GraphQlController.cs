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
        [BindRequired, FromBody] GraphQlRequest request,
        [FromServices] MyDataContext dataContext)
    {
        return Execute(dataContext, request);
    }

    [HttpGet]
    public Task<ExecutionResult> Get(
        [BindRequired, FromQuery] GraphQlRequest request,
        [FromServices] MyDataContext dataContext)
    {
        return Execute(dataContext, request);
    }

    async Task<ExecutionResult> Execute(MyDataContext dataContext, GraphQlRequest request)
    {
        var executionOptions = new ExecutionOptions
        {
            Schema = schema,
            Query = request.Query,
            OperationName = request.OperationName,
            Inputs = request.Variables.ToInputs(),
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