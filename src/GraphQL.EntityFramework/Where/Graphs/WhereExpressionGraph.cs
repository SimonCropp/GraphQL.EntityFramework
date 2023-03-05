namespace GraphQL.EntityFramework;

public class WhereExpressionGraph :
    InputObjectGraphType<WhereExpression>
{
    public WhereExpressionGraph()
    {
        Name = nameof(WhereExpression);
        Field(_ => _.Path, true);
        Field<ComparisonGraph>("comparison")
            .Resolve(_ => _.Source.Comparison);
        Field(_ => _.Negate, true);
        Field<EnumerationGraphType<StringComparison>>("case")
            .Resolve( _ => _.Source.Case);
        Field(_ => _.Value, true);
        Field<ConnectorGraph>("connector")
            .Resolve(_ => _.Source.Connector);
        Field<ListGraphType<WhereExpressionGraph>>("GroupedExpressions");
    }
}