namespace GraphQL.EntityFramework;

public class WhereExpressionGraph :
    InputObjectGraphType<WhereExpression>
{
    public WhereExpressionGraph()
    {
        Name = nameof(WhereExpression);
        Field(x => x.Path, true);
        Field<ComparisonGraph>("comparison")
            .Resolve(_ => _.Source.Comparison);
        Field(x => x.Negate, true);
        Field<EnumerationGraphType<StringComparison>>("case")
            .Resolve( _ => _.Source.Case);
        Field(x => x.Value, true);
        Field<ConnectorGraph>("connector")
            .Resolve(_ => _.Source.Connector);
        Field<ListGraphType<WhereExpressionGraph>>("GroupedExpressions");
    }
}