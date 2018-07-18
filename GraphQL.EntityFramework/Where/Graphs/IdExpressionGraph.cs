using GraphQL.Types;

class IdExpressionGraph : InputObjectGraphType<IdExpression>
{
    public IdExpressionGraph()
    {
        Field(x => x.Member, true);
        Field(x => x.Value);
    }
}