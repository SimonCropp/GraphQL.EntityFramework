using GraphQL.Execution;
using GraphQLParser.AST;
using ExecutionContext = GraphQL.Execution.ExecutionContext;

namespace GraphQL.EntityFramework;

public class EfDocumentExecuter :
    DocumentExecuter
{
    protected override IExecutionStrategy SelectExecutionStrategy(ExecutionContext context)
    {
        if (context.Operation.Operation == OperationType.Query)
        {
            return new SerialExecutionStrategy();
        }

        return base.SelectExecutionStrategy(context);
    }
}