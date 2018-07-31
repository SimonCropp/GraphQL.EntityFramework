using System.Net;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
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
        [FromBody] GraphQlQuery query,
        [FromServices] MyDataContext dataContext)
    {
        var inputs = query.Variables.ToInputs();
        return Execute(dataContext, query.Query, inputs);
    }

    [HttpGet]
    public Task<ExecutionResult> Get(
        [FromQuery] string query,
        [FromServices] MyDataContext dataContext)
    {
        return Execute(dataContext, query, null);
    }

    async Task<ExecutionResult> Execute(MyDataContext dataContext, string queryQuery, Inputs inputs)
    {
        var executionOptions = new ExecutionOptions
        {
            Schema = schema,
            Query = queryQuery,
            Inputs = inputs,
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