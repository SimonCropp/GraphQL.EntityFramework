namespace GraphQL.EntityFramework;

public class WhereExpressionGraph :
    InputObjectGraphType<WhereExpression>
{
    public WhereExpressionGraph()
    {
        Name = nameof(WhereExpression);
        Field(_ => _.Path, true);
        Field<ComparisonGraph>("comparison");
        Field(_ => _.Negate, true);
        Field(_ => _.Value, true);
        Field<ConnectorGraph>("connector");
        Field<ListGraphType<WhereExpressionGraph>>("GroupedExpressions");
    }
}