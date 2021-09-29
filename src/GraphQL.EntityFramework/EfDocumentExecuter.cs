using GraphQL.Execution;
using GraphQL.Language.AST;
using ExecutionContext = GraphQL.Execution.ExecutionContext;

namespace GraphQL.EntityFramework;

public class EfDocumentExecuter :
    DocumentExecuter
{
    protected override IExecutionStrategy SelectExecutionStrategy(ExecutionContext context)
    {
        if (context.Operation.OperationType == OperationType.Query)
        {
            return new SerialExecutionStrategy();
        }
        return base.SelectExecutionStrategy(context);
    }
}