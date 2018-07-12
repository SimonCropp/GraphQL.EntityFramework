using System;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
public class GraphQlController : Controller
{
    IDocumentExecuter documentExecuter;
    ISchema schema;

    public GraphQlController(ISchema schema, IDocumentExecuter documentExecuter)
    {
        this.schema = schema;
        this.documentExecuter = documentExecuter;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] GraphQlQuery query, [FromServices] MyDataContext dataContext)
    {
        if (query == null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        var inputs = query.Variables.ToInputs();
        var executionOptions = new ExecutionOptions
        {
            Schema = schema,
            Query = query.Query,
            Inputs = inputs,
            UserContext = dataContext
        };

        var result = await documentExecuter.ExecuteAsync(executionOptions).ConfigureAwait(false);

        if (result.Errors?.Count > 0)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}